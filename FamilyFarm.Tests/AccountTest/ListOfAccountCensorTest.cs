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
using Newtonsoft.Json.Linq;

namespace FamilyFarm.Tests.AccountTest
{
    public class ListOfAccountCensorTest
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
        public async Task GetAllAccountCensor_ReturnsUnauthorized_WhenTokenIsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.GetAllAccountByRoleId("68007b2a87b41211f0af1d57");

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("Not permission for this action.", unauthorized.Value);
        }
        [Test]
        public async Task GetAllAccountByRoleId_ReturnsNotFound_WhenListIsNull()
        {
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAllAccountByRoleId(It.IsAny<string>()))
                .ReturnsAsync((List<Account>)null);

            var result = await _controller.GetAllAccountByRoleId("68007b2a87b41211f0af1d57");

            var notFound = result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);

            var json = JObject.FromObject(notFound.Value);
            Assert.AreEqual("list not found", (string)json["message"]);
            Assert.AreEqual(false, (bool)json["success"]);
        }



        [Test]
        public async Task GetAllAccountByRoleId_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var mockList = new List<Account>
    {
        new Account
        {
            AccId = "1", Username = "censor1", Email = "censor1@example.com",
            PhoneNumber = "0123456789", RoleId = "2", Country = "Vietnam", City = "Hue",
            PasswordHash = "hash", FullName = "Censor One", CreatedAt = DateTime.UtcNow
        }
    };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAllAccountByRoleId("68007b2a87b41211f0af1d57"))
                .ReturnsAsync(mockList);

            // Act
            var result = await _controller.GetAllAccountByRoleId("68007b2a87b41211f0af1d57");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockList, okResult.Value);
        }

    }
}
