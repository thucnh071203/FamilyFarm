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

        public PaymentService(PaymentDAO paymentDAO, IBookingServiceRepository bookingRepository, IServiceRepository serviceRepository, IOptions<VNPayConfig> config)
        {
            _paymentDAO = paymentDAO;
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _config = config.Value;
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
            var vnPay = new VnPayLibrary();

            vnPay.AddRequestData("vnp_Version", "2.1.0");
            vnPay.AddRequestData("vnp_Command", "pay");
            vnPay.AddRequestData("vnp_TmnCode", _config.TmnCode); // inject config
            vnPay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
            vnPay.AddRequestData("vnp_CurrCode", "VND");
            //vnPay.AddRequestData("vnp_TxnRef", request.BookingServiceId);
            //vnPay.AddRequestData("vnp_OrderInfo", $"Thanh toan booking {request.BookingServiceId}");
            vnPay.AddRequestData("vnp_TxnRef", $"{request.BookingServiceId}_{request.SubprocessId}");
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
                SubProcessId = subprocessId,
                FromAccId = booking.AccId,
                ToAccId = service.ProviderId,
                PayAt = DateTime.Now
            };

            await _paymentDAO.CreateAsync(payment);

            booking.IsPaidByFarmer = true;
            booking.IsPaidToExpert = false;
            await _bookingRepository.UpdateBookingPayment(booking.BookingServiceId, booking);

            return true;
        }
    }

}
