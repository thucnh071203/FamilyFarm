using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FamilyFarm.BusinessLogic;

namespace FamilyFarm.Tests.PaymentTest
{
    [TestFixture]
    public class ViewListPaymentTests
    {
        //private Mock<IPaymentService> _paymentServiceMock;
        //private Mock<IBookingServiceService> _bookingServiceMock;
        //private Mock<IAuthenticationService> _authenServiceMock;
        //private Mock<IConfiguration> _configurationMock;
        //private Mock<IConfigurationSection> _vnPayConfigSectionMock;
        //private Mock<HttpContext> _httpContextMock;
        //private PaymentController _controller;

        //[SetUp]
        ////public void Setup()
        ////{
        ////    _paymentServiceMock = new Mock<IPaymentService>();
        ////    _bookingServiceMock = new Mock<IBookingServiceService>();
        ////    _authenServiceMock = new Mock<IAuthenticationService>();
        ////    _configurationMock = new Mock<IConfiguration>();
        ////    _vnPayConfigSectionMock = new Mock<IConfigurationSection>();
        ////    _httpContextMock = new Mock<HttpContext>();

        ////    // Mock configuration section for "VNPay"
        ////    _configurationMock.Setup(c => c.GetSection("VNPay"))
        ////        .Returns(_vnPayConfigSectionMock.Object);

        ////    // Mock individual configuration values for VNPay section
        ////    _vnPayConfigSectionMock.Setup(s => s["TmnCode"]).Returns("D3KUKY9D");
        ////    _vnPayConfigSectionMock.Setup(s => s["HashSecret"]).Returns("9K18D9K0LKBBMO0HP36QN2TGF59ZJE3M");
        ////    _vnPayConfigSectionMock.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        ////    _vnPayConfigSectionMock.Setup(s => s["ReturnUrl"]).Returns("http://localhost:3000/PaymentResult");
        ////    _vnPayConfigSectionMock.Setup(s => s["ReturnUrlRepayment"]).Returns("http://localhost:3000/RePaymentResult");

        ////    _controller = new PaymentController(_configurationMock.Object, _bookingServiceMock.Object, _paymentServiceMock.Object, _authenServiceMock.Object);
        ////    _controller.ControllerContext = new ControllerContext { HttpContext = _httpContextMock.Object };
        ////}

        //public void Setup()
        //{
        //    // Mock cho các service
        //    _paymentServiceMock = new Mock<IPaymentService>();
        //    _bookingServiceMock = new Mock<IBookingServiceService>();
        //    _authenServiceMock = new Mock<IAuthenticationService>();
        //    _configurationMock = new Mock<IConfiguration>();
        //    _vnPayConfigSectionMock = new Mock<IConfigurationSection>();

        //    var pdfServiceMock = new Mock<IPdfService>();  // Mock cho IPdfService
        //    var accountServiceMock = new Mock<IAccountService>();  // Mock cho IAccountService
        //    var emailSenderMock = new Mock<IEmailSender>();  // Mock cho IEmailSender

        //    // Mock configuration section for "VNPay"
        //    _configurationMock.Setup(c => c.GetSection("VNPay"))
        //        .Returns(_vnPayConfigSectionMock.Object);

        //    // Mock individual configuration values for VNPay section
        //    _vnPayConfigSectionMock.Setup(s => s["TmnCode"]).Returns("D3KUKY9D");
        //    _vnPayConfigSectionMock.Setup(s => s["HashSecret"]).Returns("9K18D9K0LKBBMO0HP36QN2TGF59ZJE3M");
        //    _vnPayConfigSectionMock.Setup(s => s["PaymentUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrl"]).Returns("http://localhost:3000/PaymentResult");
        //    _vnPayConfigSectionMock.Setup(s => s["ReturnUrlRepayment"]).Returns("http://localhost:3000/RePaymentResult");

        //    // Khởi tạo PaymentController với tất cả mock đã tạo
        //    _controller = new PaymentController(
        //        _configurationMock.Object,
        //        _bookingServiceMock.Object,
        //        _paymentServiceMock.Object,
        //        _authenServiceMock.Object,
        //        pdfServiceMock.Object,
        //        accountServiceMock.Object,
        //        emailSenderMock.Object
        //    );
        //}

        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IBookingServiceService> _bookingServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _vnPayConfigSectionMock;
        private Mock<HttpContext> _httpContextMock;
        private PaymentController _controller;

        [SetUp]
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

            // Mock HttpContext và User
            var accId = "admin01";
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, accId),
            new Claim(ClaimTypes.Role, "Admin")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(principal);

            // Ensure that the controller uses the mocked HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };

            // Mock GetDataFromToken
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });

            // Mock GetListPayment service
            var expectedResponse = new ListPaymentResponseDTO
            {
                Success = true,
                Message = "Get list payment successfully",
                Data = new List<PaymentDataMapper>
            {
                new PaymentDataMapper
                {
                    PaymentId = "payment01",
                    BookingServiceId = "booking01",
                    SubProcessId = null,
                    FromAccId = "user01",
                    ToAccId = "expert01",
                    IsRepayment = false,
                    PayAt = DateTime.UtcNow,
                    Price = 500000,
                    ServiceName = "Service A",
                    FarmerName = "John Doe",
                    ExpertName = "Jane Expert"
                }
            }
            };
            _paymentServiceMock.Setup(s => s.GetListPayment()).ReturnsAsync(expectedResponse);
        }

        [Test]
        public async Task ListPayments_AdminAuthenticated_HasPayments_ReturnsSuccessWithData()
        {
            // Arrange
            var accId = "686f1d49e010dac7cf1dbe9b";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, accId),
                new Claim(ClaimTypes.Role, "67fd41dfba121b52bbc622c3")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(principal);
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });

            var expectedResponse = new ListPaymentResponseDTO
            {
                Success = true,
                Message = "Get list payment successfully",
                Data = new List<PaymentDataMapper>
                {
                    new PaymentDataMapper
                    {
                        PaymentId = "payment01",
                        BookingServiceId = "booking01",
                        SubProcessId = null,
                        FromAccId = "user01",
                        ToAccId = "expert01",
                        IsRepayment = false,
                        PayAt = DateTime.UtcNow,
                        Price = 500000,
                        ServiceName = "Service A",
                        FarmerName = "John Doe",
                        ExpertName = "Jane Expert"
                    }
                }
            };

            _paymentServiceMock.Setup(s => s.GetListPayment()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListPayments();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListPaymentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list payment successfully", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual("payment01", response.Data[0].PaymentId);
        }

        [Test]
        public async Task ListPayments_AdminAuthenticated_NoPayments_ReturnsBadRequest()
        {
            // Arrange
            var accId = "admin01";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, accId),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(principal);
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });

            var expectedResponse = new ListPaymentResponseDTO
            {
                Success = false,
                Message = "List payment is empty",
                Data = null
            };

            _paymentServiceMock.Setup(s => s.GetListPayment()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListPayments();

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ListPaymentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("List payment is empty", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task ListPayments_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _httpContextMock.Setup(c => c.User).Returns((ClaimsPrincipal)null);
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.ListPayments();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [Test]
        public async Task ListPayments_NonAdminAuthenticated_ReturnsForbidden()
        {
            // Arrange
            var accId = "user01";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, accId),
                new Claim(ClaimTypes.Role, "Expert")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(principal);
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Expert" });

            // Act
            var result = await _controller.ListPayments();

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TearDown]
        public void TearDown()
        {
            _paymentServiceMock.Reset();
            _bookingServiceMock.Reset();
            _authenServiceMock.Reset();
            _configurationMock.Reset();
            _vnPayConfigSectionMock.Reset();
            _httpContextMock.Reset();
        }
    }
}