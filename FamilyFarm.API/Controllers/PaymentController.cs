using System.Security.Cryptography;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.BusinessLogic.VNPay;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using FamilyFarm.Models.ModelsConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly VNPayConfig _vnPayConfig;
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly IBookingServiceService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly IAuthenticationService _authenService;

        public PaymentController(IConfiguration configuration, IBookingServiceService bookingService, IPaymentService paymentService, IAuthenticationService authenService)
        {
            _vnPayConfig = configuration.GetSection("VNPay").Get<VNPayConfig>();
            _bookingService = bookingService;
            _paymentService = paymentService;
            _authenService = authenService;
        }

        [HttpGet("all-payment")]
        public async Task<IActionResult> GetAllPayments()
        {
            var result = await _paymentService.GetAllPayment();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("list-payment")]
        [Authorize]
        public async Task<IActionResult> ListPayments()
        {
            var result = await _paymentService.GetListPayment();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tạo link thanh toán VNPay và trả về cho frontend redirect
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.BookingServiceId) || request.Amount <= 0)
            {
                return BadRequest("Invalid payment data.");
            }

            try
            {
                var paymentUrl = await _paymentService.CreatePaymentUrlAsync(request, HttpContext);
                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating VNPay URL: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý callback từ VNPay sau khi thanh toán
        /// </summary>
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            try
            {
                var success = await _paymentService.HandleVNPayReturnAsync(Request.Query);
                var bookingId = Request.Query["vnp_TxnRef"].ToString();

                return Ok(new
                {
                    success = success,
                    bookingId = bookingId,
                    message = success ? "Thanh toán thành công" : "Thanh toán thất bại"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-by-bookingId/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByBookingId(string bookingId)
        {
            var payment = await _paymentService.GetPaymentByBooking(bookingId);
            return Ok(payment);
        }

        [HttpGet("get-repay-by-bookingId/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetRePaymentByBookingId(string bookingId)
        {
            var payment = await _paymentService.GetRePaymentByBooking(bookingId);
            return Ok(payment);
        }

        [HttpGet("get-by-subProcessId/{subProcessId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentBySubProcessId(string subProcessId)
        {
            var payment = await _paymentService.GetPaymentBySubProcess(subProcessId);
            return Ok(payment);
        }

        [HttpGet("get-repay-by-subprocessId/{subprocessId}")]
        [Authorize]
        public async Task<IActionResult> GetRePaymentBySubProcessId(string subprocessId)
        {
            var payment = await _paymentService.GetRePaymentBySubprocess(subprocessId);
            return Ok(payment);
        }

        [HttpPost("create-repayment")]
        [Authorize]
        public async Task<IActionResult> CreateRepayment([FromBody] CreateRepaymentRequestDTO request)
        {
            Console.WriteLine("Repayment API");
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            request.AdminId = accId;

            if (string.IsNullOrEmpty(request.BookingServiceId) || request.Amount <= 0)
                return BadRequest("Invalid repayment data.");

            try
            {
                var url = await _paymentService.CreateRepaymentUrlAsync(request, HttpContext);
                return Ok(new { paymentUrl = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating repayment URL: {ex.Message}");
            }
        }

        [HttpGet("vnpay-return-repayment")]
        public async Task<IActionResult> VNPayReturnRepayment()
        {
            Console.WriteLine("Return Repayment API");
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            try
            {
                var success = await _paymentService.HandleRepaymentVNPayReturnAsync(Request.Query);
                var txnRef = Request.Query["vnp_TxnRef"].ToString();

                return Ok(new
                {
                    success = success,
                    txnRef = txnRef,
                    message = success ? "Hoàn trả expert thành công" : "Hoàn trả thất bại"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

    }
}
