using AutoMapper;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.AccountTest
{
    public class ResponseRequestOfExpertTest
    {
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IAccountService> _accountService;
        private IMapper _mapper;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _accountService = new Mock<IAccountService>();
            _authenticationService = new Mock<IAuthenticationService>();


            _controller = new AccountController(_accountService.Object, _authenticationService.Object, _mapper);

        }

        [Test]
        public async Task ResponseRequestExpert_ReturnsUnauthorized_WhenAccIdIsNull()
        {
            // Arrange
            var accId = "682b59a71e71bf40fe1cfe6f";
            var status = 1;

            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null);

            var controller = new AccountController(_accountService.Object, _authenticationService.Object, _mapper);

            // Act
            var result = await controller.UpdateAccountStatus(accId, status);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual("Not permission for this action.", unauthorizedResult.Value);
        }

        [Test]
        public async Task ResponseRequestExpert_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            // Arrange
            var accId = "682b59a71e71bf40fe1cfe6f";
            var status = 1;
            _accountService.Setup(s => s.UpdateAccountStatus(accId, status)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAccountStatus(accId, status);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(true, okResult.Value);
        }

        [Test]
        public async Task ResponseRequestExpert_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            // Arrange
            var accId = "682b59a71e71bf40fe1cfe6f";
            var status = 1;
            _accountService.Setup(s => s.UpdateAccountStatus(accId, status)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateAccountStatus(accId, status);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("have some error when update status of censor!", badRequestResult.Value);
        }
        [Test]
        public async Task ResponseRequestExpert_ReturnsOk_WhenUpdateIsSuccessful_Allow()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            // Arrange
            var accId = "682b59a71e71bf40fe1cfe6f";
            var status = 0;
            _accountService.Setup(s => s.UpdateAccountStatus(accId, status)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAccountStatus(accId, status);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(true, okResult.Value);
        }

        [Test]
        public async Task ResponseRequestExpert_ReturnsBadRequest_WhenUpdateFails_Allow()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            // Arrange
            var accId = "682b59a71e71bf40fe1cfe6f";
            var status = 0;
            _accountService.Setup(s => s.UpdateAccountStatus(accId, status)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateAccountStatus(accId, status);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("have some error when update status of censor!", badRequestResult.Value);
        }
    }
}
