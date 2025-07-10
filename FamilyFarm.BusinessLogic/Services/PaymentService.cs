using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.VNPay;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Models.ModelsConfig;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace FamilyFarm.BusinessLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDAO _paymentDAO;
        private readonly IBookingServiceRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly VNPayConfig _config;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRevenueRepository _revenueRepository;

        public PaymentService(PaymentDAO paymentDAO, IBookingServiceRepository bookingRepository, IServiceRepository serviceRepository, IOptions<VNPayConfig> config, IPaymentRepository paymentRepository, IRevenueRepository revenueRepository)
        {
            _paymentDAO = paymentDAO;
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _config = config.Value;
            _paymentRepository = paymentRepository;
            _revenueRepository = revenueRepository;
        }

        public async Task<PaymentResponseDTO> GetAllPayment()
        {
            var data = await _paymentDAO.GetAllAsync();
            return new PaymentResponseDTO
            {
                Success = true,
                Data = data
            };
        }

        public async Task<PaymentTransaction> GetPayment(string id)
        {
            return await _paymentDAO.GetByIdAsync(id);
        }

        public async Task<PaymentTransaction> CreatePayment(PaymentTransaction transaction)
        {
            return await _paymentDAO.CreateAsync(transaction);
        }

        public async Task<string> CreatePaymentUrlAsync(CreatePaymentRequestDTO request, HttpContext httpContext)
        {
            Console.WriteLine("Chao thanh toan");
            Console.WriteLine(request.Amount);
            var vnPay = new VnPayLibrary();

            vnPay.AddRequestData("vnp_Version", "2.1.0");
            vnPay.AddRequestData("vnp_Command", "pay");
            vnPay.AddRequestData("vnp_TmnCode", _config.TmnCode); // inject config
            vnPay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
            vnPay.AddRequestData("vnp_CurrCode", "VND");
            //vnPay.AddRequestData("vnp_TxnRef", request.BookingServiceId);
            //vnPay.AddRequestData("vnp_OrderInfo", $"Thanh toan booking {request.BookingServiceId}");
            //vnPay.AddRequestData("vnp_TxnRef", $"{request.BookingServiceId}_{request.SubprocessId}");
            var txnRef = string.IsNullOrWhiteSpace(request.SubprocessId)
            ? request.BookingServiceId
            : $"{request.BookingServiceId}_{request.SubprocessId}";

            vnPay.AddRequestData("vnp_TxnRef", txnRef);
            vnPay.AddRequestData("vnp_OrderInfo", $"Thanh toan booking {request.BookingServiceId}, subprocess {request.SubprocessId}");
            vnPay.AddRequestData("vnp_OrderType", "other");
            vnPay.AddRequestData("vnp_Locale", "vn");
            vnPay.AddRequestData("vnp_ReturnUrl", _config.ReturnUrl);
            vnPay.AddRequestData("vnp_IpAddr", httpContext.Connection.RemoteIpAddress?.ToString());
            vnPay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnPay.CreateRequestUrl(_config.PaymentUrl, _config.HashSecret);
            return paymentUrl;
        }


        public async Task<bool> HandleVNPayReturnAsync(IQueryCollection vnpayData)
        {
            var vnp_SecureHash = vnpayData["vnp_SecureHash"];
            var inputData = new SortedList<string, string>();

            //foreach (var key in vnpayData.Keys)
            //{
            //    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
            //    {
            //        //inputData.Add(key, vnpayData[key]);
            //        inputData.Add(key, vnpayData[key].ToString());
            //    }
            //}

            foreach (var key in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(key) &&
                    key.StartsWith("vnp_") &&
                    key != "vnp_SecureHash" &&
                    key != "vnp_SecureHashType")
                {
                    inputData.Add(key, vnpayData[key].ToString());
                }
            }

            //var rawData = string.Join("&", inputData.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var rawData = string.Join("&", inputData.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            var computedHash = VnPayLibrary.HmacSHA512(_config.HashSecret, rawData); // inject config nếu cần

            //Console.WriteLine("VNPay rawData: " + rawData);
            //Console.WriteLine("VNPay computedHash: " + computedHash);
            //Console.WriteLine("VNPay provided hash: " + vnp_SecureHash);


            if (!string.Equals(computedHash, vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string responseCode = vnpayData["vnp_ResponseCode"];
            //string bookingServiceId = vnpayData["vnp_TxnRef"];
            string txnRef = vnpayData["vnp_TxnRef"];
            string[] parts = txnRef.Split('_');
            string bookingServiceId = parts[0];
            string subprocessId = parts.Length > 1 ? parts[1] : null;

            if (responseCode != "00") return false;

            var booking = await _bookingRepository.GetById(bookingServiceId);
            if (booking == null) return false;

            var service = await _serviceRepository.GetServiceById(booking.ServiceId);
            if (service == null) return false;

            var payment = new PaymentTransaction
            {
                PaymentId = ObjectId.GenerateNewId().ToString(),
                BookingServiceId = booking.BookingServiceId,
                SubProcessId = !string.IsNullOrEmpty(subprocessId) ? subprocessId : null, // ✅ Chỉ gán nếu không rỗng,
                FromAccId = booking.AccId,
                ToAccId = service.ProviderId,
                IsRepayment = false,
                PayAt = DateTime.Now
            };

            await _paymentDAO.CreateAsync(payment);

            // Tạo mới doanh thu nếu chưa có dữ liệu, có rồi thì trả về dữ liệu có rồi
            await _revenueRepository.CreateNewRevenue();

            // ✅ Tính số tiền vừa thanh toán
            decimal amount = decimal.Parse(vnpayData["vnp_Amount"]) / 100;
            Console.WriteLine("Kiểm tra payment");
            Console.WriteLine(amount);

            // ✅ Ví dụ: 10% là hoa hồng
            decimal commission = amount * 0.10m;

            await _revenueRepository.ChangeRevenue(amount, commission);

            booking.IsPaidByFarmer = true;
            booking.IsPaidToExpert = false;
            await _bookingRepository.UpdateBookingPayment(booking.BookingServiceId, booking);

            return true;
        }

        public async Task<PaymentResponseDTO> GetPaymentByBooking(string bookingId)
        {
            var payment = await _paymentRepository.GetPaymentByBooking(bookingId);

            if (payment == null)
            {
                return new PaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found payment by booking"
                };
            }

            return new PaymentResponseDTO
            {
                Success = true,
                Message = "Get payment by booking success",
                Data = new List<PaymentTransaction> { payment }
            };
        }

        public async Task<PaymentResponseDTO> GetPaymentBySubProcess(string processId)
        {
            var payment = await _paymentRepository.GetPaymentBySubProcess(processId);

            if (payment == null)
            {
                return new PaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found payment by sub process"
                };
            }

            return new PaymentResponseDTO
            {
                Success = true,
                Message = "Get payment by sub process success",
                Data = new List<PaymentTransaction> { payment }
            };
        }

    }
}
