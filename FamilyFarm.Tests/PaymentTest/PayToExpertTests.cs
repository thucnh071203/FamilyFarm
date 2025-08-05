using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FamilyFarm.BusinessLogic;

namespace FamilyFarm.Tests.PaymentTest
{
    [TestFixture]
    public class PayToExpertTests
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IBookingServiceService> _bookingServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _vnPayConfigSectionMock;
        private Mock<HttpContext> _httpContextMock; // Mock HttpContext
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
            var accId = "686f1d49e010dac7cf1dbe9b";
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, accId),
            new Claim(ClaimTypes.Role, "67fd41dfba121b52bbc622c3")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            // Mock HttpContext
            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(c => c.User).Returns(principal);

            // Cập nhật ControllerContext để controller sử dụng HttpContext mock
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_ValidData_ExpertExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "686f1d49e010dac7cf1dbe9b";
            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "685bcf25de5f40459858fa80",
                SubprocessId = "sub01",
                Amount = 500000,
                AdminId = accId
            };
            var paymentUrl = "https://vnpay.vn/payment?txnRef=685bcf25de5f40459858fa80_sub01_repay";
            _paymentServiceMock.Setup(s => s.CreateRepaymentUrlAsync(It.IsAny<CreateRepaymentRequestDTO>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(paymentUrl);

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult should not be null.");
            Assert.IsNotNull(okResult.Value, "Response value should not be null.");
            Assert.AreEqual(200, okResult.StatusCode);

            // Use reflection to access the paymentUrl property from the anonymous type
            var responseType = okResult.Value.GetType();
            var paymentUrlProperty = responseType.GetProperty("paymentUrl");
            Assert.IsNotNull(paymentUrlProperty, "Response must contain 'paymentUrl' property.");
            var actualPaymentUrl = paymentUrlProperty.GetValue(okResult.Value) as string;
            Assert.IsNotNull(actualPaymentUrl, "paymentUrl should not be null.");
            Assert.AreEqual(paymentUrl, actualPaymentUrl, "paymentUrl does not match expected value.");
        }

        [Test]
        public async Task CreateRepayment_NotAuthenticated_ReturnsOk()
        {
            // Arrange
            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "685bcf25de5f40459858fa80",
                SubprocessId = "sub01",
                Amount = 500000,
                AdminId = "admin01"
            };
            var paymentUrl = "https://vnpay.vn/payment?txnRef=685bcf25de5f40459858fa80_sub01_repay";
            _httpContextMock.Setup(c => c.User).Returns((ClaimsPrincipal)null);
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            _paymentServiceMock.Setup(s => s.CreateRepaymentUrlAsync(It.IsAny<CreateRepaymentRequestDTO>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(paymentUrl);

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult should not be null.");
            Assert.IsNotNull(okResult.Value, "Response value should not be null.");
            Assert.AreEqual(200, okResult.StatusCode);

            // Use reflection to access the paymentUrl property from the anonymous type
            var responseType = okResult.Value.GetType();
            var paymentUrlProperty = responseType.GetProperty("paymentUrl");
            Assert.IsNotNull(paymentUrlProperty, "Response must contain 'paymentUrl' property.");
            var actualPaymentUrl = paymentUrlProperty.GetValue(okResult.Value) as string;
            Assert.IsNotNull(actualPaymentUrl, "paymentUrl should not be null.");
            Assert.AreEqual(paymentUrl, actualPaymentUrl, "paymentUrl does not match expected value.");
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_ExpertDoesNotExist_ReturnsBadRequest()
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

            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "2983457928387834",
                SubprocessId = "sub01",
                Amount = 500000,
                AdminId = accId
            };
            _paymentServiceMock.Setup(s => s.CreateRepaymentUrlAsync(It.IsAny<CreateRepaymentRequestDTO>(), It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Expert does not exist"));

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Error creating repayment URL: Expert does not exist", objectResult.Value);
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_ExpertInactive_ReturnsBadRequest()
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

            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "68443a5f4f3f0edb4e5969e5",
                SubprocessId = "sub01",
                Amount = 500000,
                AdminId = accId
            };
            _paymentServiceMock.Setup(s => s.CreateRepaymentUrlAsync(It.IsAny<CreateRepaymentRequestDTO>(), It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Expert is inactive"));

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Error creating repayment URL: Expert is inactive", objectResult.Value);
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_EmptyBookingServiceId_ReturnsBadRequest()
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

            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "",
                SubprocessId = "sub01",
                Amount = 500000,
                AdminId = accId
            };

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid repayment data.", badRequestResult.Value);
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_ZeroAmount_ReturnsBadRequest()
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

            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "685bcf25de5f40459858fa80",
                SubprocessId = "sub01",
                Amount = 0,
                AdminId = accId
            };

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid repayment data.", badRequestResult.Value);
        }

        [Test]
        public async Task CreateRepayment_AdminAuthenticated_ExcessiveAmount_ReturnsBadRequest()
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

            var request = new CreateRepaymentRequestDTO
            {
                BookingServiceId = "685bcf25de5f40459858fa80",
                SubprocessId = "sub01",
                Amount = 99999999999,
                AdminId = accId
            };
            _paymentServiceMock.Setup(s => s.CreateRepaymentUrlAsync(It.IsAny<CreateRepaymentRequestDTO>(), It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Amount exceeds maximum limit"));

            // Act
            var result = await _controller.CreateRepayment(request);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Error creating repayment URL: Amount exceeds maximum limit", objectResult.Value);
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