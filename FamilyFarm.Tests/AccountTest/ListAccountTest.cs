using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic;
using FamilyFarm.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using NUnit.Framework;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Tests.AccountTest
{
    public class ListAccountTest
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
        public async Task GetAllAccount_ReturnsUnauthorized_WhenTokenIsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.GetAllAccount();

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("Not permission for this action.", unauthorized.Value);
        }
        [Test]
        public async Task GetAllAccount_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAllAccountExceptAdmin())
                .ReturnsAsync((List<Account>?)null);

            var result = await _controller.GetAllAccount();

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("have some error when get account information!", badRequest.Value);
        }

        [Test]
        public async Task GetAllAccount_ReturnsOk_WhenSuccessful()
        {
            var mockData = new List<Account>
    {
        new Account
        {
            AccId = "6810e3831b27b2917c58d77c", Username = "testuser", FullName = "Test User",
            Email = "test@example.com", PhoneNumber = "123456789",
            Country = "Vietnam", City = "Hanoi", PasswordHash = "hash",
            RoleId = "2", CreatedAt = DateTime.UtcNow, Status = 1
        }
    };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _accountService.Setup(x => x.GetAllAccountExceptAdmin())
                .ReturnsAsync(mockData);

            var result = await _controller.GetAllAccount();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockData, okResult.Value);
        }


    }
}
