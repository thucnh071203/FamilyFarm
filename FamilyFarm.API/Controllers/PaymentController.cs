using FamilyFarm.BusinessLogic.VNPay;
using FamilyFarm.Models.ModelsConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly VNPayConfig _vnPayConfig;
        private readonly VnPayLibrary _vnPayLibrary;

        public PaymentController(IConfiguration configuration)
        {
            _vnPayConfig = configuration.GetSection("VNPay").Get<VNPayConfig>();
        }

        [HttpGet("create")]
        public IActionResult CreatePayment(decimal amount)
        {
            string vnp_TransactionRef = DateTime.Now.Ticks.ToString(); // Mã giao dịch
            string vnp_OrderInfo = "Thanh toan don hang #" + vnp_TransactionRef;
            string vnp_Amount = ((int)(amount * 100)).ToString(); // nhân 100 vì VNPay yêu cầu đơn vị là VND * 100

            string vnp_TmnCode = _vnPayConfig.TmnCode;
            string vnp_HashSecret = _vnPayConfig.HashSecret;
            string vnp_Url = _vnPayConfig.PaymentUrl;
            string vnp_ReturnUrl = _vnPayConfig.ReturnUrl;

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", vnp_Amount);
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", vnp_OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", vnp_TransactionRef);

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Ok(new { url = paymentUrl });
        }

        [HttpGet("vnpay-return")]
        public IActionResult VNPayReturn()
        {
            var vnpayData = Request.Query;
            var vnp_SecureHash = vnpayData["vnp_SecureHash"];
            var inputData = new SortedList<string, string>();

            foreach (var key in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    inputData.Add(key, vnpayData[key]);
                }
            }

            var rawData = string.Join("&", inputData.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            //var checkHash = new VnPayLibrary();
            // Hàm này là static
            var computedHash = VnPayLibrary.HmacSHA512(_vnPayConfig.HashSecret, rawData);

            if (computedHash == vnp_SecureHash)
            {
                string status = vnpayData["vnp_ResponseCode"];
                return Ok(new { success = status == "00", message = status == "00" ? "Thanh toán thành công" : "Thanh toán thất bại" });
            }

            return BadRequest("Sai checksum");
        }

    }
}
