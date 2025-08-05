using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyFarm.Tests.PaymentTest
{
    [TestFixture]
    public class PayForBookingServiceTest
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IBookingServiceService> _bookingServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _vnPayConfigSectionMock;
        private PaymentController _controller;

        [SetUp]
        //public void Setup()
        //{
        //    _paymentServiceMock = new Mock<IPaymentService>();
        //    _bookingServiceMock = new Mock<IBookingServiceService>();
        //    _authenServiceMock = new Mock<IAuthenticationService>();
        //    _configurationMock = new Mock<IConfiguration>();
        //    _vnPayConfigSectionMock = new Mock<IConfigurationSection>();

        //    // Mock configuration section for "VNPay"
        //    _configurationMock.Setup(c => c.GetSection("VNPay"))
        //        .Returns(_vnPayConfigSectionMock.Object);

        //    // Mock individual configuration values for VNPay section
        //    _vnPayConfigSectionMock.Setup(s => s["TmnCode"]).Returns("D3KUKY9D");
        //    _vnPayConfigSectionMock.Setup(s => s["HashSecret"]).Returns("9K18D9K0LKBBMO0HP36QN2TGF59ZJE3M");
        //    _vnPayConfigSectionMock.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrl"]).Returns("http://localhost:3000/PaymentResult");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrlRepayment"]).Returns("http://localhost:3000/RePaymentResult");

        //    _controller = new PaymentController(_configurationMock.Object, _bookingServiceMock.Object, _paymentServiceMock.Object, _authenServiceMock.Object);
        //}

        //public void Setup()
        //{
        //    _paymentServiceMock = new Mock<IPaymentService>();
        //    _bookingServiceMock = new Mock<IBookingServiceService>();
        //    _authenServiceMock = new Mock<IAuthenticationService>();
        //    _configurationMock = new Mock<IConfiguration>();
        //    _vnPayConfigSectionMock = new Mock<IConfigurationSection>();
        //    var pdfServiceMock = new Mock<IPdfService>();  // Thêm mock cho IPdfService

        //    // Mock configuration section for "VNPay"
        //    _configurationMock.Setup(c => c.GetSection("VNPay"))
        //        .Returns(_vnPayConfigSectionMock.Object);

        //    // Mock individual configuration values for VNPay section
        //    _vnPayConfigSectionMock.Setup(s => s["TmnCode"]).Returns("D3KUKY9D");
        //    _vnPayConfigSectionMock.Setup(s => s["HashSecret"]).Returns("9K18D9K0LKBBMO0HP36QN2TGF59ZJE3M");
        //    _vnPayConfigSectionMock.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrl"]).Returns("http://localhost:3000/PaymentResult");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrlRepayment"]).Returns("http://localhost:3000/RePaymentResult");

        //    _controller = new PaymentController(_configurationMock.Object, _bookingServiceMock.Object, _paymentServiceMock.Object, _authenServiceMock.Object, pdfServiceMock.Object, _accountServiceMock.Object, _emailSenderMock.Object); // Thêm mock vào constructor
        //}

        public void Setup()
        {
            // Mock cho các service
            _paymentServiceMock = new Mock<IPaymentService>();
            _bookingServiceMock = new Mock<IBookingServiceService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _configurationMock = new Mock<IConfiguration>();
            _vnPayConfigSectionMock = new Mock<IConfigurationSection>();

            var pdfServiceMock = new Mock<IPdfService>();  // Mock cho IPdfService
            var accountServiceMock = new Mock<IAccountService>();  // Mock cho IAccountService
            var emailSenderMock = new Mock<IEmailSender>();  // Mock cho IEmailSender

            // Mock configuration section for "VNPay"
            _configurationMock.Setup(c => c.GetSection("VNPay"))
                .Returns(_vnPayConfigSectionMock.Object);

            // Mock individual configuration values for VNPay section
            _vnPayConfigSectionMock.Setup(s => s["TmnCode"]).Returns("D3KUKY9D");
            _vnPayConfigSectionMock.Setup(s => s["HashSecret"]).Returns("9K18D9K0LKBBMO0HP36QN2TGF59ZJE3M");
            _vnPayConfigSectionMock.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
            _vnPayConfigSectionMock.Setup(s => s["ReturnUrl"]).Returns("http://localhost:3000/PaymentResult");
            _vnPayConfigSectionMock.Setup(s => s["ReturnUrlRepayment"]).Returns("http://localhost:3000/RePaymentResult");

            // Khởi tạo PaymentController với tất cả mock đã tạo
            _controller = new PaymentController(
                _configurationMock.Object,
                _bookingServiceMock.Object,
                _paymentServiceMock.Object,
                _authenServiceMock.Object,
                pdfServiceMock.Object,
                accountServiceMock.Object,
                emailSenderMock.Object
            );
        }


        [Test]
        public async Task CreatePayment_ValidBooking_ReturnsPaymentUrl()
        {
            var request = new CreatePaymentRequestDTO
            {
                BookingServiceId = "booking123",
                Amount = 500000
            };

            _paymentServiceMock.Setup(x => x.CreatePaymentUrlAsync(request, It.IsAny<HttpContext>()))
                .ReturnsAsync("https://paymentgateway.com/pay/123");

            var result = await _controller.CreatePayment(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult!.Value);

            var json = JsonConvert.SerializeObject(okResult.Value);
            var jsonObj = JObject.Parse(json);

            Assert.IsTrue(jsonObj.ContainsKey("paymentUrl"));
            Assert.IsTrue(jsonObj["paymentUrl"]!.ToString()!.Contains("paymentgateway.com"));
        }


        [Test]
        public async Task CreatePayment_InvalidBookingId_ReturnsBadRequest()
        {
            var request = new CreatePaymentRequestDTO
            {
                BookingServiceId = "",
                Amount = 500000
            };

            var result = await _controller.CreatePayment(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task CreatePayment_BookingNotFound_Returns500()
        {
            var request = new CreatePaymentRequestDTO
            {
                BookingServiceId = "1225472078305702",
                Amount = 500000
            };

            _paymentServiceMock.Setup(x => x.CreatePaymentUrlAsync(request, It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Booking not found"));

            var result = await _controller.CreatePayment(request);

            Assert.IsInstanceOf<ObjectResult>(result);
            var serverError = result as ObjectResult;
            Assert.AreEqual(500, serverError!.StatusCode);
            Assert.IsTrue(serverError.Value!.ToString()!.Contains("Booking not found"));
        }

        [Test]
        public async Task CreatePayment_BookingOfOtherUser_Returns500()
        {
            var request = new CreatePaymentRequestDTO
            {
                BookingServiceId = "6879f271063c89af67e56a52",
                Amount = 500000
            };

            _paymentServiceMock.Setup(x => x.CreatePaymentUrlAsync(request, It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("You don't have permission for this booking"));

            var result = await _controller.CreatePayment(request);

            Assert.IsInstanceOf<ObjectResult>(result);
            var forbidden = result as ObjectResult;
            Assert.AreEqual(500, forbidden!.StatusCode);
            Assert.IsTrue(forbidden.Value!.ToString()!.Contains("permission"));
        }

        [Test]
        public async Task CreatePayment_BookingAlreadyPaid_Returns500()
        {
            var request = new CreatePaymentRequestDTO
            {
                BookingServiceId = "687a04d7063c89af67e56a58",
                Amount = 500000
            };

            _paymentServiceMock.Setup(x => x.CreatePaymentUrlAsync(request, It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Booking already paid"));

            var result = await _controller.CreatePayment(request);

            Assert.IsInstanceOf<ObjectResult>(result);
            var paid = result as ObjectResult;
            Assert.AreEqual(500, paid!.StatusCode);
            Assert.IsTrue(paid.Value!.ToString()!.Contains("already paid"));
        }
    }
}
