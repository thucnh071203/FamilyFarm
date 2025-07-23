using AutoMapper;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
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
    public class ViewAccountDetailTest
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
        public async Task GetAccountByAccId_ReturnsUnauthorized_WhenAccIdIsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null);

            var controller = new AccountController(_accountService.Object, _authenticationService.Object, _mapper);

            // Act
            var result = await controller.GetAccountByAccId("682b59a71e71bf40fe1cfe6f");

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual("Not permission for this action.", unauthorizedResult.Value);
        }
        [Test]
        public async Task GetAccountByAccId_ReturnsBadRequest_WhenAccountIsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAccountByAccId("682b59a71e71bf40fe1cfe6f"))
                .ReturnsAsync((Account)null);

            var controller = new AccountController(_accountService.Object, _authenticationService.Object, _mapper);

            // Act
            var result = await controller.GetAccountByAccId("682b59a71e71bf40fe1cfe6f");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("have some error when get account information!", badRequestResult.Value);
        }
        [Test]
        public async Task GetAccountByAccId_ReturnsOk_WhenAccountExists()
        {
            // Arrange
            var accId = "acc123";
            var expectedAccount = new Account { AccId = accId, Email = "test@example.com" };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAccountByAccId(accId))
                .ReturnsAsync(expectedAccount);

            var controller = new AccountController(_accountService.Object, _authenticationService.Object, _mapper);

            // Act
            var result = await controller.GetAccountByAccId(accId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedAccount = okResult.Value as Account;
            Assert.IsNotNull(returnedAccount);
            Assert.AreEqual(accId, returnedAccount.AccId);
            Assert.AreEqual("test@example.com", returnedAccount.Email);
        }

    }
}
