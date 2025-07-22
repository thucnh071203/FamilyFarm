using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.AuthenticationTest
{
    [TestFixture]
    public class SetPasswordTest
    {
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IAccountService> _accountServiceMock;
        private PasswordHasher _passwordHasher;
        private Mock<IEmailSender> _emailSenderMock;
        private AuthenticationController _controller;

        [SetUp]
        public void Setup()
        {
            _authenServiceMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _passwordHasher = new PasswordHasher();
            _emailSenderMock = new Mock<IEmailSender>();

            _controller = new AuthenticationController(
                _authenServiceMock.Object,
                _accountServiceMock.Object,
                _passwordHasher,
                _emailSenderMock.Object
            );
        }

        [Test]
        public async Task SetPassword_WithNoToken_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            var request = new SetPasswordDTO { Password = "", ConfirmPassword = "" };

            var result = await _controller.SetPassword(request);
            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task SetPassword_WithEmptyPassword_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });
            _controller.ModelState.AddModelError("Password", "Required");
            var request = new SetPasswordDTO { Password = "", ConfirmPassword = "" };

            var result = await _controller.SetPassword(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task SetPassword_WithWrongConfirmPassword_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            var account = new Account { AccId = "acc123", Otp = -1 };
            _accountServiceMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(account);

            _controller.ModelState.AddModelError("ConfirmPassword", "Password does not match!");

            var request = new SetPasswordDTO { Password = "abc1237", ConfirmPassword = "wrongconfirm" };

            var result = await _controller.SetPassword(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task SetPassword_WithAlreadySetPassword_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            var account = new Account { AccId = "acc123", Otp = 123456 };
            _accountServiceMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(account);

            var request = new SetPasswordDTO { Password = "newpassword123", ConfirmPassword = "newpassword123" };

            var result = await _controller.SetPassword(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task SetPassword_WithValidInput_ReturnsOk()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            var account = new Account { AccId = "acc123", Otp = -1 };
            _accountServiceMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(account);
            _accountServiceMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(account);

            var request = new SetPasswordDTO { Password = "newpassword123", ConfirmPassword = "newpassword123" };

            var result = await _controller.SetPassword(request);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Password setted successfully!", okResult.Value);
        }
    }
}
