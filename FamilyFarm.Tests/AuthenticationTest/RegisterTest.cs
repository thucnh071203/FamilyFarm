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
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.AuthenticationTest
{
    public class RegisterTest
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
        public async Task RegisterFarmer_WithInvalidModelState_ReturnsBadRequest()
        {
            var request = new RegisterFarmerRequestDTO();
            _controller.ModelState.AddModelError("FullName", "Required");

            var result = await _controller.RegisterFarmer(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task RegisterFarmer_WithInvalidEmailAndPhone_ReturnsBadRequestWithErrors()
        {
            var request = new RegisterFarmerRequestDTO
            {
                FullName = "Tran Phuong Nam",
                Email = "email.com",
                Phone = "abc",
                Address = "An Giang",
                Username = "user123",
                Password = "newpassword123"
            };

            _authenServiceMock.Setup(x => x.CheckValidEmail(request.Email)).Returns(false);
            _authenServiceMock.Setup(x => x.CheckValidPhoneNumber(request.Phone)).Returns(false);

            var result = await _controller.RegisterFarmer(request);

            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var badRequest = result.Result as ObjectResult;
            Assert.AreEqual(400, badRequest.StatusCode);
            var errors = badRequest.Value as List<string>;
            Assert.Contains("This email is invalid!", errors);
            Assert.Contains("This phone is invalid!", errors);
        }

        [Test]
        public async Task RegisterFarmer_WithValidInput_ReturnsOk()
        {
            var request = new RegisterFarmerRequestDTO
            {
                FullName = "Tran Phuong Nam",
                Email = "namtpce170126@fpt.edu.vn",
                Phone = "0816560543",
                Address = "An Giang",
                Username = "user123",
                Password = "newpassword123"
            };

            _authenServiceMock.Setup(x => x.CheckValidEmail(request.Email)).Returns(true);
            _authenServiceMock.Setup(x => x.CheckValidPhoneNumber(request.Phone)).Returns(true);

            _authenServiceMock.Setup(x => x.RegisterFarmer(request)).ReturnsAsync(new RegisterFarmerResponseDTO { IsSuccess = true });

            var result = await _controller.RegisterFarmer(request);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as RegisterFarmerResponseDTO;
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task RegisterFarmer_WithDuplicateUsername_ReturnsBadRequest()
        {
            var request = new RegisterFarmerRequestDTO
            {
                FullName = "Tran Phuong Nam",
                Email = "validemail@gmail.com",
                Phone = "0816560543",
                Address = "An Giang",
                Username = "existinguser",
                Password = "newpassword123"
            };

            _authenServiceMock.Setup(x => x.CheckValidEmail(request.Email)).Returns(true);
            _authenServiceMock.Setup(x => x.CheckValidPhoneNumber(request.Phone)).Returns(true);
            _authenServiceMock.Setup(x => x.RegisterFarmer(request)).ReturnsAsync(new RegisterFarmerResponseDTO { IsSuccess = false, MessageError = "Username has been registered" });

            var result = await _controller.RegisterFarmer(request);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var badRequest = result.Result as ObjectResult;
            Assert.AreEqual(400, badRequest.StatusCode);
            var response = badRequest.Value as RegisterFarmerResponseDTO;
            Assert.AreEqual("Username has been registered", response.MessageError);
        }

        [Test]
        public async Task RegisterFarmer_WithDuplicateEmail_ReturnsBadRequest()
        {
            var request = new RegisterFarmerRequestDTO
            {
                FullName = "Tran Phuong Nam",
                Email = "duplicate@gmail.com",
                Phone = "0816560543",
                Address = "An Giang",
                Username = "user123",
                Password = "newpassword123"
            };

            _authenServiceMock.Setup(x => x.CheckValidEmail(request.Email)).Returns(true);
            _authenServiceMock.Setup(x => x.CheckValidPhoneNumber(request.Phone)).Returns(true);
            _authenServiceMock.Setup(x => x.RegisterFarmer(request)).ReturnsAsync(new RegisterFarmerResponseDTO { IsSuccess = false, MessageError = "Email has been registered" });

            var result = await _controller.RegisterFarmer(request);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var badRequest = result.Result as ObjectResult;
            Assert.AreEqual(400, badRequest.StatusCode);
            var response = badRequest.Value as RegisterFarmerResponseDTO;
            Assert.AreEqual("Email has been registered", response.MessageError);
        }

        [Test]
        public async Task RegisterFarmer_WithDuplicatePhone_ReturnsBadRequest()
        {
            var request = new RegisterFarmerRequestDTO
            {
                FullName = "Tran Phuong Nam",
                Email = "validemail@gmail.com",
                Phone = "duplicatedphone",
                Address = "An Giang",
                Username = "user123",
                Password = "newpassword123"
            };

            _authenServiceMock.Setup(x => x.CheckValidEmail(request.Email)).Returns(true);
            _authenServiceMock.Setup(x => x.CheckValidPhoneNumber(request.Phone)).Returns(true);
            _authenServiceMock.Setup(x => x.RegisterFarmer(request)).ReturnsAsync(new RegisterFarmerResponseDTO { IsSuccess = false, MessageError = "Phone has been registered" });

            var result = await _controller.RegisterFarmer(request);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var badRequest = result.Result as ObjectResult;
            Assert.AreEqual(400, badRequest.StatusCode);
            var response = badRequest.Value as RegisterFarmerResponseDTO;
            Assert.AreEqual("Phone has been registered", response.MessageError);
        }

    }
}
