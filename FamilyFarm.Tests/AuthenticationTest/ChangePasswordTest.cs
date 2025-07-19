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
    public class ChangePasswordTest
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
        public async Task ChangePassword_WithNoToken_ReturnsBadRequest()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var request = new ChangePasswordDTO
            {
                OldPassword = "oldpassword",
                NewPassword = "newpassword",
                ConfirmPassword = "newpassword"
            };

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task ChangePassword_WithWrongOldPassword_ReturnsBadRequest()
        {
            // Arrange
            var userClaims = new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" };

            // Hash 1 password hợp lệ
            var wrongPasswordHash = _passwordHasher.HashPassword("correctpassword");

            var account = new Account
            {
                AccId = "acc123",
                RoleId = "role123",
                Username = "testuser",
                PasswordHash = wrongPasswordHash,
                FullName = "Test User",
                Email = "test@gmail.com",
                PhoneNumber = "0123456789",
                City = "Hanoi",
                Country = "Vietnam",
                Status = 0,
                IsFacebook = false,
                CreatedAt = DateTime.UtcNow
            };

            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(userClaims);

            _accountServiceMock.Setup(x => x.GetAccountById(userClaims.AccId))
                .ReturnsAsync(account);

            var request = new ChangePasswordDTO
            {
                OldPassword = "wrongpassword",
                NewPassword = "newpassword",
                ConfirmPassword = "newpassword"
            };

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Password wrong!", badRequestResult!.Value);
        }

        [Test]
        public async Task ChangePassword_WithEmptyFields_ReturnsBadRequest()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            // mock account hợp lệ
            var account = new Account
            {
                AccId = "acc123",
                RoleId = "role123",
                Username = "testuser",
                PasswordHash = _passwordHasher.HashPassword("oldpassword"),
                FullName = "Test User",
                Email = "test@gmail.com",
                PhoneNumber = "0123456789",
                City = "Hanoi",
                Country = "Vietnam",
                Status = 1,
                IsFacebook = false,
                CreatedAt = DateTime.UtcNow
            };

            _accountServiceMock.Setup(x => x.GetAccountById("acc123"))
                .ReturnsAsync(account);

            var request = new ChangePasswordDTO
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };

            _controller.ModelState.AddModelError("OldPassword", "Required");
            _controller.ModelState.AddModelError("NewPassword", "Required");
            _controller.ModelState.AddModelError("ConfirmPassword", "Required");

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ChangePassword_WithShortNewPassword_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            var correctPassword = "oldpassword";
            var passwordHash = _passwordHasher.HashPassword(correctPassword);

            var account = new Account
            {
                AccId = "acc123",
                RoleId = "role123",
                Username = "testuser",
                PasswordHash = passwordHash,
                FullName = "Test User",
                Email = "test@gmail.com",
                PhoneNumber = "0123456789",
                City = "Hanoi",
                Country = "Vietnam",
                Status = 1,
                IsFacebook = false,
                CreatedAt = DateTime.UtcNow
            };

            _accountServiceMock.Setup(x => x.GetAccountById("acc123"))
                .ReturnsAsync(account);

            var request = new ChangePasswordDTO
            {
                OldPassword = correctPassword, // khớp để qua verify
                NewPassword = "abc1237",        // dưới 8 ký tự
                ConfirmPassword = "abc1237"
            };

            _controller.ModelState.AddModelError("NewPassword", "Password must be at least 8 characters");

            var result = await _controller.ChangePassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ChangePassword_WithValidData_ReturnsOk()
        {
            var userClaims = new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" };

            var correctPassword = "correctpass";
            var passwordHash = _passwordHasher.HashPassword(correctPassword);

            var account = new Account
            {
                AccId = "acc123",
                RoleId = "role123",
                Username = "testuser",
                PasswordHash = passwordHash,
                FullName = "Test User",
                Email = "test@gmail.com",
                PhoneNumber = "0123456789",
                City = "Hanoi",
                Country = "Vietnam",
                Status = 1,
                IsFacebook = false,
                CreatedAt = DateTime.UtcNow
            };

            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(userClaims);
            _accountServiceMock.Setup(x => x.GetAccountById(userClaims.AccId)).ReturnsAsync(account);
            _accountServiceMock.Setup(x => x.UpdateAsync(userClaims.AccId, It.IsAny<Account>())).ReturnsAsync(account);

            var request = new ChangePasswordDTO
            {
                OldPassword = correctPassword,
                NewPassword = "newvalidpassword",
                ConfirmPassword = "newvalidpassword"
            };

            var result = await _controller.ChangePassword(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Password changed successfully!", okResult!.Value);
        }

        [Test]
        public async Task ChangePassword_WithInvalidFormatPassword_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "acc123", Username = "testuser" });

            var request = new ChangePasswordDTO
            {
                OldPassword = "12345678",
                NewPassword = "new password 123", // giả sử sai format (không ký tự đặc biệt)
                ConfirmPassword = "newpassword123"
            };

            _controller.ModelState.AddModelError("NewPassword", "Password is not in correct format for password");
            _controller.ModelState.AddModelError("ConfirmPassword", "Confirm password does not match");
            var result = await _controller.ChangePassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

    }
}
