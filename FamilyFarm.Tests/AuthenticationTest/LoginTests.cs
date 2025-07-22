using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.PasswordHashing;

namespace FamilyFarm.Tests.AuthenticationTest
{
    [TestFixture]
    public class LoginTests
    {
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IAccountService> _accountServiceMock;
        private Mock<PasswordHasher> _passwordHasherMock;
        private Mock<IEmailSender> _emailSenderMock;
        private AuthenticationController _controller;

        [SetUp]
        public void Setup()
        {
            _authenServiceMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _passwordHasherMock = new Mock<PasswordHasher>();
            _emailSenderMock = new Mock<IEmailSender>();

            _controller = new AuthenticationController(
                _authenServiceMock.Object,
                _accountServiceMock.Object,
                _passwordHasherMock.Object,
                _emailSenderMock.Object
            );
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Identifier = "user1",
                Password = "password1"
            };

            var expectedResponse = new LoginResponseDTO
            {
                AccId = "123",
                Username = "user1",
                AccessToken = "valid-token",
                TokenExpiryIn = 3600
            };

            _authenServiceMock
                .Setup(x => x.Login(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result, "Result should not be null.");
                Assert.IsNotNull(okResult, "Expected OkObjectResult.");
                Assert.AreEqual(200, okResult?.StatusCode ?? 200, "Expected status code 200.");
                Assert.AreEqual(expectedResponse, okResult?.Value, "Returned value should match the expected LoginResponseDTO.");
            });
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedResult()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Identifier = "user@example.com",
                Password = "wrong-password"
            };

            _authenServiceMock
                .Setup(x => x.Login(request))
                .ReturnsAsync((LoginResponseDTO?)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<UnauthorizedResult>(result.Result, "Expected UnauthorizedResult.");
                Assert.AreEqual(401, ((UnauthorizedResult)result.Result).StatusCode, "Expected status code 401.");
            });
        }

        [Test]
        public async Task Login_WithLockedAccount_ReturnsLockedStatus()
        {
            // Arrange
            var request = new LoginRequestDTO
            {
                Identifier = "locked@example.com",
                Password = "password123"
            };

            var lockedResponse = new LoginResponseDTO
            {
                Message = "Account is locked login.",
                LockedUntil = DateTime.UtcNow.AddMinutes(10)
            };

            _authenServiceMock
                .Setup(x => x.Login(request))
                .ReturnsAsync(lockedResponse);

            // Act
            var result = await _controller.Login(request);
            var statusResult = result.Result as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.IsNotNull(statusResult);
                Assert.AreEqual(423, statusResult?.StatusCode, "Exp ected status code 423 (Locked).");
                Assert.AreEqual(lockedResponse, statusResult?.Value, "Returned value should match locked account response.");
            });
        }

        [TearDown]
        public void TearDown()
        {
            _authenServiceMock?.Reset();
            _accountServiceMock?.Reset();
            _passwordHasherMock?.Reset();
            _emailSenderMock?.Reset();
        }
    }
}
