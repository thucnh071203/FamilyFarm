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
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.AuthenticationTest
{
    [TestFixture]
    public class ResetPasswordTest
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
        public async Task ForgotPassword_AccountNotFound_ReturnsNotFound()
        {
            var request = new ResetPasswordDTO { AccId = "notfound", Password = "newpassword123" };
            _accountServiceMock.Setup(x => x.GetAccountById(request.AccId)).ReturnsAsync((Account)null);

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_AccountLocked_ReturnsBadRequest()
        {
            var request = new ResetPasswordDTO { AccId = "acc123", Password = "newpassword123" };
            var account = new Account { AccId = "acc123", Status = 1 };
            _accountServiceMock.Setup(x => x.GetAccountById(request.AccId)).ReturnsAsync(account);

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Account is inactivate.", badRequest.Value);
        }

        [Test]
        public async Task ForgotPassword_ValidRequest_ReturnsOk()
        {
            var request = new ResetPasswordDTO { AccId = "acc123", Password = "newpassword123" };
            var account = new Account { AccId = "acc123", Status = 0 };
            _accountServiceMock.Setup(x => x.GetAccountById(request.AccId)).ReturnsAsync(account);

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Password reset successfully!", okResult.Value);
        }

        [Test]
        public async Task ForgotPassword_WithEmptyPassword_ReturnsBadRequest()
        {
            var request = new ResetPasswordDTO { AccId = "acc123", Password = "" , ConfirmPassword = ""};
            _controller.ModelState.AddModelError("Password", "Required");
            _controller.ModelState.AddModelError("ConfirmPassword", "Required");

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WithShortPassword_ReturnsBadRequest()
        {
            var request = new ResetPasswordDTO { AccId = "acc123", Password = "short7" };
            _controller.ModelState.AddModelError("Password", "Password must be at least 8 characters long.");

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WithTooLongPassword_ReturnsBadRequest()
        {
            var longPassword = new string('a', 300);
            var request = new ResetPasswordDTO { AccId = "acc123", Password = longPassword };
            _controller.ModelState.AddModelError("Password", "Password exceeds maximum allowed length.");

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WithPasswordContainingSpaces_ReturnsBadRequest()
        {
            var request = new ResetPasswordDTO { AccId = "acc123", Password = "new password" };
            _controller.ModelState.AddModelError("Password", "Password cannot contain spaces.");

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

    }
}
