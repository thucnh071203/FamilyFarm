using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.AuthenticationTest
{
    [TestFixture]
    public class LogoutTest
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
        public async Task Logout_WithValidToken()
        {
            // Arrange
            var userClaims = new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" };
            var expectedResponse = new LoginResponseDTO
            {
                AccessToken = null,
                RefreshToken = null,
                Message = "Logout successfully."
            };

            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(userClaims);

            _authenServiceMock.Setup(x => x.Logout(userClaims.Username))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOf<ActionResult<LoginResponseDTO>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult!.Value as LoginResponseDTO;
            Assert.IsNotNull(response);
            Assert.AreEqual("Logout successfully.", response!.Message);
        }

        [Test]
        public async Task Logout_WithInvalidToken_ReturnsBadRequest()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsInstanceOf<ActionResult<LoginResponseDTO>>(result);
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }
    }
   

}
