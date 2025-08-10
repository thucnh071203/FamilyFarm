using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.VNPay;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Models.ModelsConfig;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IProcessRepository _processRepository;
        private readonly IRepaymentCacheService _repaymentCacheService;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryServiceRepository _categoryServiceRepository;
        private readonly IStatisticService _statisticService;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;

        public PaymentService(PaymentDAO paymentDAO, IBookingServiceRepository bookingRepository, IServiceRepository serviceRepository, IOptions<VNPayConfig> config, IPaymentRepository paymentRepository, IRevenueRepository revenueRepository, IProcessRepository processRepository, IRepaymentCacheService repaymentCacheService, IAccountRepository accountRepository, ICategoryServiceRepository categoryServiceRepository, IStatisticService statisticService, IHubContext<TopEngagedPostHub> hubContext)
        {
            _paymentDAO = paymentDAO;
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _config = config.Value;
            _paymentRepository = paymentRepository;
            _revenueRepository = revenueRepository;
            _processRepository = processRepository;
            _repaymentCacheService = repaymentCacheService;
            _accountRepository = accountRepository;
            _categoryServiceRepository = categoryServiceRepository;
            _statisticService = statisticService;
            _hubContext = hubContext;
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

            var rawData = string.Join("&", inputData.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            var computedHash = VnPayLibrary.HmacSHA512(_config.HashSecret, rawData); // inject config nếu cần

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

            var existedSubProcessPayment = await _paymentRepository.GetPaymentBySubProcess(subprocessId);
            if (existedSubProcessPayment != null && existedSubProcessPayment.IsRepayment == false)
            {
                Console.WriteLine("⚠️ Đã tồn tại thanh toán cho subprocess này, không xử lý lại.");
                return true;
            }

            var existedBookingPayment = await _paymentRepository.GetPaymentByBooking(bookingServiceId);
            if (existedBookingPayment != null && existedBookingPayment.IsRepayment == false)
            {
                Console.WriteLine("⚠️ Đã tồn tại thanh toán cho booking này, không xử lý lại.");
                return true;
            }

            if (responseCode != "00") return false;

            var booking = await _bookingRepository.GetById(bookingServiceId);
            if (booking == null) return false;

            //var service = await _serviceRepository.GetServiceById(booking.ServiceId);
            //if (service == null) return false;

            var payment = new PaymentTransaction
            {
                PaymentId = ObjectId.GenerateNewId().ToString(),
                BookingServiceId = booking.BookingServiceId,
                SubProcessId = !string.IsNullOrEmpty(subprocessId) ? subprocessId : null, // ✅ Chỉ gán nếu không rỗng,
                FromAccId = booking.AccId,
                ToAccId = booking.ExpertId,
                IsRepayment = false,
                PayAt = DateTime.Now
            };

            await _paymentDAO.CreateAsync(payment);

            // Tạo mới doanh thu nếu chưa có dữ liệu, có rồi thì trả về dữ liệu có rồi
            await _revenueRepository.CreateNewRevenue();

            // ✅ Tính số tiền vừa thanh toán
            decimal amount = decimal.Parse(vnpayData["vnp_Amount"]) / 100;
            //Console.WriteLine("Kiểm tra payment");
            //Console.WriteLine(amount);

            // ✅ Ví dụ: 10% là hoa hồng
            decimal commission = amount * 0.10m;

            await _revenueRepository.ChangeRevenue(amount, commission);

            // return ngay tại đây và cập nhật thời gian thanh toán
            if (!string.IsNullOrEmpty(subprocessId))
            {
                var subprocess = await _processRepository.GetSubProcessBySubProcessId(subprocessId);
                if (subprocess == null) return false;

                subprocess.PayAt = DateTime.Now;

                await _processRepository.UpdateSubProcess(subprocessId, subprocess);
                return true;
            }

            booking.BookingServiceStatus = "Paid";
            booking.IsPaidByFarmer = true;
            booking.IsPaidToExpert = false;
            await _bookingRepository.UpdateBookingPayment(booking.BookingServiceId, booking);

            var updatedBooking = await _bookingRepository.GetById(booking.BookingServiceId);
            //await _hubContext.Clients.All.SendAsync("BookingUpdated", updatedBooking);
            //await _hubContext.Clients.All.SendAsync("BookingPaid", booking);
            await _hubContext.Clients.Group(booking.ExpertId).SendAsync("BookingPaid", booking);

            var revenueData = await _statisticService.GetSystemRevenueAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveRevenueUpdate", revenueData);


            var expertRevenue = await _statisticService.GetRevenueByExpertAsync(booking.ExpertId, null, null);
            await _hubContext.Clients.All.SendAsync("ReceiveExpertRevenueUpdate", expertRevenue);

            return true;
        }

        public async Task<string> CreateRepaymentUrlAsync(CreateRepaymentRequestDTO request, HttpContext httpContext)
        {
            Console.WriteLine($"AdminId:{request.AdminId}");

            var vnPay = new VnPayLibrary();

            vnPay.AddRequestData("vnp_Version", "2.1.0");
            vnPay.AddRequestData("vnp_Command", "pay");
            vnPay.AddRequestData("vnp_TmnCode", _config.TmnCode);
            vnPay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
            vnPay.AddRequestData("vnp_CurrCode", "VND");

            string txnRef = $"{request.BookingServiceId}_{request.SubprocessId}_repay";

            //string baseRef = string.IsNullOrEmpty(request.SubprocessId)
            //? request.BookingServiceId
            //: $"{request.BookingServiceId}_{request.SubprocessId}";

            //string txnRef = $"{baseRef}_repay_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";


            //var txnRef = string.IsNullOrWhiteSpace(request.SubprocessId)
            //? $"{request.BookingServiceId}_repay"
            //: $"{request.BookingServiceId}_{request.SubprocessId}_repay";
            //var baseTxnRef = string.IsNullOrWhiteSpace(request.SubprocessId)
            //? request.BookingServiceId
            //: $"{request.BookingServiceId}_{request.SubprocessId}";

            //// Đảm bảo txnRef là duy nhất bằng cách thêm timestamp ticks
            //var uniqueTxnRef = $"{baseTxnRef}_repay_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            //string txnRef = $"{request.BookingServiceId}_{request.SubprocessId}_repay_{DateTime.Now.Ticks}";


            //_repaymentCacheService.SaveAdminId(txnRef, request.AdminId);
            vnPay.AddRequestData("vnp_TxnRef", txnRef);
            //vnPay.AddRequestData("vnp_OrderInfo", $"Hoàn trả cho expert - booking {request.BookingServiceId}, subprocess {request.SubprocessId}");
            vnPay.AddRequestData("vnp_OrderInfo",
            $"repay_booking:{request.BookingServiceId};sub:{request.SubprocessId ?? "null"};admin:{request.AdminId}");
            vnPay.AddRequestData("vnp_OrderType", "other");
            vnPay.AddRequestData("vnp_Locale", "vn");
            vnPay.AddRequestData("vnp_ReturnUrl", _config.ReturnUrlRepayment);
            vnPay.AddRequestData("vnp_IpAddr", httpContext.Connection.RemoteIpAddress?.ToString());
            vnPay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            return vnPay.CreateRequestUrl(_config.PaymentUrl, _config.HashSecret);
        }

        public async Task<bool> HandleRepaymentVNPayReturnAsync(IQueryCollection vnpayData)
        {
            var vnp_SecureHash = vnpayData["vnp_SecureHash"];
            var inputData = new SortedList<string, string>();

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

            var rawData = string.Join("&", inputData.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            var computedHash = VnPayLibrary.HmacSHA512(_config.HashSecret, rawData);

            if (!string.Equals(computedHash, vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string responseCode = vnpayData["vnp_ResponseCode"];
            if (responseCode != "00") return false;

            string txnRef = vnpayData["vnp_TxnRef"];
            string cleanRef = txnRef.Replace("_repay", "");
            string[] parts = cleanRef.Split('_');
            string bookingServiceId = parts[0];
            string subprocessId = parts.Length > 1 ? parts[1] : null;

            string orderInfo = vnpayData["vnp_OrderInfo"];
            var matches = Regex.Match(orderInfo, @"admin:(?<adminId>[a-zA-Z0-9]+)");

            var adminId = matches.Success ? matches.Groups["adminId"].Value : "ADMIN";


            //string txnRef = vnpayData["vnp_TxnRef"]; // ví dụ: 6866f65fcf5d0ea120d93d2d_sub123_repay_1720803019384

            //Console.WriteLine($"➡️ txnRef: {txnRef}");
            //Console.WriteLine($"➡️ bookingServiceId: {bookingServiceId}");
            //Console.WriteLine($"➡️ subprocessId: {subprocessId ?? "(null)"}");
            //Console.WriteLine($"➡️ adminId: {adminId ?? "(null)"}");


            // Tránh duplicate repayment
            var existedPaymentBooking = await _paymentRepository.GetRepaymentByBookingId(bookingServiceId);
            if (existedPaymentBooking != null && existedPaymentBooking.IsRepayment == true)
            {
                Console.WriteLine("⚠️ Repayment booking đã tồn tại.");
                return true;
            }

            var existedPaymentSubProcess = await _paymentRepository.GetRepaymentBySubProcessId(subprocessId);
            if (existedPaymentBooking != null && existedPaymentBooking.IsRepayment == true)
            {
                Console.WriteLine("⚠️ Repayment subprocess đã tồn tại.");
                return true;
            }

            var booking = await _bookingRepository.GetById(bookingServiceId);
            if (booking == null) return false;

            decimal amount = decimal.Parse(vnpayData["vnp_Amount"]) / 100;

            decimal? commission = amount * 0.10m;

            decimal? finalAmount = amount - commission;

            //var adminId = _repaymentCacheService.GetAdminId(txnRef) ?? "ADMIN"; // fallback nếu cache mất

            //var adminId = AdminId;

            Console.WriteLine($"{adminId}");

            var repayment = new PaymentTransaction
            {
                PaymentId = ObjectId.GenerateNewId().ToString(),
                BookingServiceId = booking.BookingServiceId,
                SubProcessId = !string.IsNullOrEmpty(subprocessId) ? subprocessId : null,
                FromAccId = adminId, // 👈 Gán cứng hoặc lấy từ context nếu có phân quyền
                ToAccId = booking.ExpertId,
                IsRepayment = true,
                PayAt = DateTime.Now
            };

            await _paymentDAO.CreateAsync(repayment);

            // Tạo mới doanh thu nếu chưa có dữ liệu, có rồi thì trả về dữ liệu có rồi
            await _revenueRepository.CreateNewRevenue();

            await _revenueRepository.ChangeRevenue(-finalAmount, 0);

            // Trả về ngay tại đây không cần update thêm
            if (!string.IsNullOrEmpty(subprocessId))
            {
                return true;
            }

            // Tạm thời vì chưa có chức năng complete booking
            //booking.BookingServiceStatus = "Completed";
            booking.IsPaidToExpert = true;
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

        public async Task<PaymentResponseDTO> GetRePaymentByBooking(string bookingId)
        {
            var payment = await _paymentRepository.GetRepaymentByBookingId(bookingId);

            if (payment == null)
            {
                return new PaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found repayment by booking"
                };
            }

            return new PaymentResponseDTO
            {
                Success = true,
                Message = "Get repayment by booking success",
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

        public async Task<PaymentResponseDTO> GetRePaymentBySubprocess(string subprocessId)
        {
            var payment = await _paymentRepository.GetRepaymentBySubProcessId(subprocessId);

            if (payment == null)
            {
                return new PaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found repayment by subprocess"
                };
            }

            return new PaymentResponseDTO
            {
                Success = true,
                Message = "Get repayment by subprocess success",
                Data = new List<PaymentTransaction> { payment }
            };
        }

        public async Task<ListPaymentResponseDTO> GetListPayment()
        {
            var listPayment = await _paymentDAO.GetAllAsync();

            var sortedList = listPayment
                .OrderByDescending(p => p.PayAt) // hoặc PayAt, PaymentDate tùy vào tên trường của bạn
                .ToList();

            if (listPayment == null || !listPayment.Any())
            {
                return new ListPaymentResponseDTO
                {
                    Success = false,
                    Message = "List payment is empty",
                };
            }

            var result = new List<PaymentDataMapper>();

            foreach (var payment in sortedList)
            {
                string? serviceName = null;
                string? farmerName = null;
                string? expertName = null;
                decimal? price = null;

                if (!string.IsNullOrEmpty(payment.SubProcessId))
                {
                    //var getPaymentBySubProcess = await _paymentRepository.GetPaymentBySubProcess(payment.SubProcessId);
                    var getPayment = await _paymentRepository.GetPayment(payment.PaymentId);
                    var subProcess = await _processRepository.GetSubProcessBySubProcessId(payment.SubProcessId);
                    var booking = await _bookingRepository.GetById(payment.BookingServiceId);
                    var farmer = await _accountRepository.GetAccountByAccId(getPayment.FromAccId);
                    var expert = await _accountRepository.GetAccountByAccId(getPayment.ToAccId);
                    var service = await _serviceRepository.GetByIdOutDelete(booking.ServiceId);

                    serviceName = service?.ServiceName;
                    farmerName = farmer?.FullName;
                    expertName = expert?.FullName;
                    price = subProcess?.Price;
                }
                else
                {
                    var getPayment = await _paymentRepository.GetPayment(payment.PaymentId);
                    var booking = await _bookingRepository.GetById(payment.BookingServiceId);
                    var farmer = await _accountRepository.GetAccountByAccId(getPayment.FromAccId);
                    var expert = await _accountRepository.GetAccountByAccId(getPayment.ToAccId);
                    var service = await _serviceRepository.GetByIdOutDelete(booking.ServiceId);

                    serviceName = service?.ServiceName;
                    farmerName = farmer?.FullName;
                    expertName = expert?.FullName;
                    price = booking?.Price;
                }

                result.Add(new PaymentDataMapper
                {
                    PaymentId = payment.PaymentId,
                    BookingServiceId = payment.BookingServiceId,
                    SubProcessId = payment.SubProcessId,
                    FromAccId = payment.FromAccId,
                    ToAccId = payment.ToAccId,
                    IsRepayment = payment.IsRepayment,
                    PayAt = payment.PayAt,
                    Price = price,
                    ServiceName = serviceName,
                    FarmerName = farmerName,
                    ExpertName = expertName
                });
            }

            return new ListPaymentResponseDTO
            {
                Success = true,
                Message = "Get list payment successfully",
                Data = result
            };
        }

        //public async Task<RepaymentResponseDTO> RepayToExpertAsync(CreateRepaymentRequestDTO request)
        //{
        //    // Request null
        //    if (request == null || (string.IsNullOrEmpty(request.SubprocessId) && string.IsNullOrEmpty(request.BookingServiceId))) {
        //        return new RepaymentResponseDTO
        //        {
        //            Success = false,
        //            Message = "Invalid request"
        //        };
        //    }

        //    string message = "";
        //    PaymentTransaction payment = null!;

        //    // SubProcessId khác null hoặc rỗng
        //    if (!string.IsNullOrEmpty(request.SubprocessId))
        //    {
        //        var getSubProcess = await _processRepository.GetSubProcessBySubProcessId(request.SubprocessId);
        //        if (getSubProcess == null)
        //        {
        //            return new RepaymentResponseDTO
        //            {
        //                Success = false,
        //                Message = "SubProcess not found"
        //            };
        //        }

        //        decimal? amount = getSubProcess.Price;

        //        decimal? commission = amount * 0.10m;

        //        decimal? finalAmount = amount - commission;

        //        // Tạo Repayment cho subProcess
        //        payment = new PaymentTransaction
        //        {
        //            PaymentId = ObjectId.GenerateNewId().ToString(),
        //            BookingServiceId = request.BookingServiceId,
        //            SubProcessId = request.SubprocessId,
        //            FromAccId = request.AdminId,
        //            ToAccId = getSubProcess.ExpertId,
        //            IsRepayment = true,
        //            PayAt = DateTime.Now
        //        };

        //        await _paymentDAO.CreateAsync(payment);

        //        // Tạo mới doanh thu nếu chưa có dữ liệu, có rồi thì trả về dữ liệu có rồi
        //        await _revenueRepository.CreateNewRevenue();

        //        await _revenueRepository.ChangeRevenue(-finalAmount, 0);

        //        message = "Repayment for subprocess successful";
        //    } else if (!string.IsNullOrEmpty(request.BookingServiceId))
        //    {
        //        var getBooking = await _bookingRepository.GetById(request.BookingServiceId);
        //        if (getBooking == null)
        //        {
        //            return new RepaymentResponseDTO
        //            {
        //                Success = false,
        //                Message = "Booking not found"
        //            };
        //        }

        //        decimal? amount = getBooking.Price;

        //        decimal? commission = amount * 0.10m;

        //        decimal? finalAmount = amount - commission;

        //        // Tạo Repayment cho subProcess
        //        payment = new PaymentTransaction
        //        {
        //            PaymentId = ObjectId.GenerateNewId().ToString(),
        //            BookingServiceId = request.BookingServiceId,
        //            SubProcessId = null,
        //            FromAccId = request.AdminId,
        //            ToAccId = getBooking.ExpertId,
        //            IsRepayment = true,
        //            PayAt = DateTime.Now
        //        };

        //        await _paymentDAO.CreateAsync(payment);

        //        // Tạo mới doanh thu nếu chưa có dữ liệu, có rồi thì trả về dữ liệu có rồi
        //        await _revenueRepository.CreateNewRevenue();

        //        await _revenueRepository.ChangeRevenue(-finalAmount, 0);

        //        // Cập nhật booking sau khi repayment
        //        getBooking.IsPaidToExpert = true;

        //        await _bookingRepository.UpdateBooking(request.BookingServiceId, getBooking);

        //        message = "Repayment for booking successful";
        //    }

        //    return new RepaymentResponseDTO
        //    {
        //        Success = true,
        //        Message = message,
        //        Data = payment
        //    };
        //}

        public async Task<BillPaymentResponseDTO> GetBillPayment(string? paymentId)
        {
            var payment = await _paymentRepository.GetPayment(paymentId);

            if (payment == null)
            {
                return new BillPaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found payment."
                };
            }

            var getBooking = await _bookingRepository.GetById(payment.BookingServiceId);
            var getService = await _serviceRepository.GetByIdOutDelete(getBooking.ServiceId);
            var getServiceCate = await _categoryServiceRepository.GetCategoryServiceById(getService.CategoryServiceId);
            var getPayer = await _accountRepository.GetAccountByAccId(payment.FromAccId);
            var getFarmer = await _accountRepository.GetAccountByAccId(getBooking.AccId);
            var getExpert = await _accountRepository.GetAccountByAccId(getService.ProviderId);

            decimal? finalPrice = 0;
            if (payment.IsRepayment != null && payment.IsRepayment == true) {
                finalPrice = getBooking.Price * 0.10m;
            } else
            {
                finalPrice = getBooking?.Price;
            }

            var result = new BillPaymentMapper
            {
                PaymentId = payment.PaymentId,
                BookingServiceId = payment.BookingServiceId,
                SubProcessId = payment.SubProcessId,
                FromAccId = payment.FromAccId,
                ToAccId = payment.ToAccId,
                IsRepayment = payment.IsRepayment,
                PayAt = payment.PayAt,
                Price = finalPrice,

                ServiceName = getService?.ServiceName,
                CategoryServiceName = getServiceCate?.CategoryName,

                ExpertName = getExpert?.FullName,
                FarmerName = getFarmer?.FullName,
                PayerName = getPayer?.FullName,

                BookingServiceAt = getBooking?.BookingServiceAt
            };

            return new BillPaymentResponseDTO
            {
                Success = true,
                Message = "Get bill payment success",
                Data = result
            };
        }

        public async Task<PaymentTransaction> GetLatestPaymentOfPayerById(string fromAccId)
        {
            var result = await _paymentRepository.GetLatestPaymentOfPayerById(fromAccId);

            return result;
        }

        public async Task<BillPaymentResponseDTO> GetBillPaymentByBookingId(string? bookingId)
        {
            var payment = await _paymentRepository.GetPaymentByBooking(bookingId);

            if (payment == null)
            {
                return new BillPaymentResponseDTO
                {
                    Success = false,
                    Message = "Can not found payment."
                };
            }

            var getBooking = await _bookingRepository.GetById(payment.BookingServiceId);
            var getService = await _serviceRepository.GetByIdOutDelete(getBooking.ServiceId);
            var getServiceCate = await _categoryServiceRepository.GetCategoryServiceById(getService.CategoryServiceId);
            var getPayer = await _accountRepository.GetAccountByAccId(payment.FromAccId);
            var getFarmer = await _accountRepository.GetAccountByAccId(getBooking.AccId);
            var getExpert = await _accountRepository.GetAccountByAccId(getService.ProviderId);

            decimal? finalPrice = 0;
            if (payment.IsRepayment != null && payment.IsRepayment == true)
            {
                finalPrice = getBooking.Price * 0.10m;
            }
            else
            {
                finalPrice = getBooking?.Price;
            }

            var result = new BillPaymentMapper
            {
                PaymentId = payment.PaymentId,
                BookingServiceId = payment.BookingServiceId,
                SubProcessId = payment.SubProcessId,
                FromAccId = payment.FromAccId,
                ToAccId = payment.ToAccId,
                IsRepayment = payment.IsRepayment,
                PayAt = payment.PayAt,
                Price = finalPrice,

                ServiceName = getService?.ServiceName,
                CategoryServiceName = getServiceCate?.CategoryName,

                ExpertName = getExpert?.FullName,
                FarmerName = getFarmer?.FullName,
                PayerName = getPayer?.FullName,

                BookingServiceAt = getBooking?.BookingServiceAt
            };

            return new BillPaymentResponseDTO
            {
                Success = true,
                Message = "Get bill payment success",
                Data = result
            };
        }

        public async Task<ListPaymentResponseDTO> GetListPaymentUser(string accId)
        {
            var listPayment = await _paymentDAO.GetAllAsync();

            if (listPayment == null || !listPayment.Any())
            {
                return new ListPaymentResponseDTO
                {
                    Success = false,
                    Message = "List payment is empty"
                };
            }

            // ✅ Lọc theo FromAccId
            var filteredList = listPayment
                .Where(p => p.FromAccId == accId)
                .OrderByDescending(p => p.PayAt)
                .ToList();

            if (!filteredList.Any())
            {
                return new ListPaymentResponseDTO
                {
                    Success = false,
                    Message = $"No payments found for FromAccId: {accId}"
                };
            }

            var result = new List<PaymentDataMapper>();

            foreach (var payment in filteredList)
            {
                string? serviceName = null;
                string? farmerName = null;
                string? expertName = null;
                decimal? price = null;

                if (!string.IsNullOrEmpty(payment.SubProcessId))
                {
                    //var getPaymentBySubProcess = await _paymentRepository.GetPaymentBySubProcess(payment.SubProcessId);
                    var getPayment = await _paymentRepository.GetPayment(payment.PaymentId);
                    var subProcess = await _processRepository.GetSubProcessBySubProcessId(payment.SubProcessId);
                    var booking = await _bookingRepository.GetById(payment.BookingServiceId);
                    var farmer = await _accountRepository.GetAccountByAccId(getPayment.FromAccId);
                    var expert = await _accountRepository.GetAccountByAccId(getPayment.ToAccId);
                    var service = await _serviceRepository.GetByIdOutDelete(booking.ServiceId);

                    serviceName = service?.ServiceName;
                    farmerName = farmer?.FullName;
                    expertName = expert?.FullName;
                    price = subProcess?.Price;
                }
                else
                {
                    var getPayment = await _paymentRepository.GetPayment(payment.PaymentId);
                    var booking = await _bookingRepository.GetById(payment.BookingServiceId);
                    var farmer = await _accountRepository.GetAccountByAccId(getPayment.FromAccId);
                    var expert = await _accountRepository.GetAccountByAccId(getPayment.ToAccId);
                    var service = await _serviceRepository.GetByIdOutDelete(booking.ServiceId);

                    serviceName = service?.ServiceName;
                    farmerName = farmer?.FullName;
                    expertName = expert?.FullName;
                    price = booking?.Price;
                }

                result.Add(new PaymentDataMapper
                {
                    PaymentId = payment.PaymentId,
                    BookingServiceId = payment.BookingServiceId,
                    SubProcessId = payment.SubProcessId,
                    FromAccId = payment.FromAccId,
                    ToAccId = payment.ToAccId,
                    IsRepayment = payment.IsRepayment,
                    PayAt = payment.PayAt,
                    Price = price,
                    ServiceName = serviceName,
                    FarmerName = farmerName,
                    ExpertName = expertName
                });
            }

            return new ListPaymentResponseDTO
            {
                Success = true,
                Message = "Get list payment successfully",
                Data = result
            };
        }
    }
}
