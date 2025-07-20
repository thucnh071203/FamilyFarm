using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
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

namespace FamilyFarm.Tests.FriendTest
{
    public class FollowExpertTest
    {
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IAccountService> _accountServiceMock;
        private FriendController _friendController;
        private Mock<IFriendService> _friendService;
        private Mock<IFriendRequestService> _friendRequestService;

        [SetUp]
        public void Setup()
        {
            _authenticationService = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _friendService = new Mock<IFriendService>();
            _friendRequestService = new Mock<IFriendRequestService>();

            _friendController = new FriendController(
              _friendRequestService.Object,
              _friendService.Object,
              _accountServiceMock.Object,
              _authenticationService.Object

                );


        }

        [Test]//test not log in
        public async Task FollowExpert_ReturnsUnauthorized_WhenUsernameIsNull()
        {
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null);

            var result = await _friendController.SendFriendRequest(new CreateFriendRequestDTO { ReceiverId = "6810e3831b27b2917c58d77c" });

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        // receiver id is null
        [Test]
        public async Task SendFriendRequest_ReturnsBadRequest_WhenReceiverIdIsNull()
        {
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "testuser" });

            _accountServiceMock.Setup(a => a.GetAccountByUsername("testuser"))
                .ReturnsAsync(new Account
                {
                    AccId = "6843e30d3c4871a0339bb1a9", // ✅ valid ObjectId string
                    RoleId = "64b68e6dfceab1d3c0bca3a2", // also must be valid if used
                    Username = "testuser",
                    PasswordHash = "hashed",
                    FullName = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "123456789",
                    City = "TestCity",
                    Country = "TestCountry",
                    Status = 1,
                    IsFacebook = false,
                    CreatedAt = DateTime.UtcNow
                });

            var result = await _friendController.SendFriendRequest(new CreateFriendRequestDTO { ReceiverId = null });

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("ReceiverId không hợp lệ.", badRequest.Value);
        }


        [Test]
        public async Task SendFriendRequest_ReturnsOk_WhenSuccessful()
        {
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "testuser" });

            _accountServiceMock.Setup(a => a.GetAccountByUsername("testuser"))
                .ReturnsAsync(new Account
                {
                    AccId = "6843e30d3c4871a0339bb1a9", // ✅ valid ObjectId string
                    RoleId = "64b68e6dfceab1d3c0bca3a2", // also must be valid if used
                    Username = "testuser",
                    PasswordHash = "hashed",
                    FullName = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "123456789",
                    City = "TestCity",
                    Country = "TestCountry",
                    Status = 1,
                    IsFacebook = false,
                    CreatedAt = DateTime.UtcNow
                });

            _friendRequestService.Setup(f => f.SendFriendRequestAsync("6843e30d3c4871a0339bb1a9", "6810e3831b27b2917c58d77c"))
                .ReturnsAsync(true);

            var result = await _friendController.SendFriendRequest(new CreateFriendRequestDTO { ReceiverId = "6810e3831b27b2917c58d77c" });

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as FriendRequestResponse;
            Assert.IsTrue(response.IsSuccess);
        }

    }
}
