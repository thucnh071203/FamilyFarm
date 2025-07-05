using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.BusinessLogic.VNPay;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using FamilyFarm.Models.ModelsConfig;
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

        public PaymentController(IConfiguration configuration, IBookingServiceService bookingService, IPaymentService paymentService)
        {
            _vnPayConfig = configuration.GetSection("VNPay").Get<VNPayConfig>();
            _bookingService = bookingService;
            _paymentService = paymentService;
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

    }
}
