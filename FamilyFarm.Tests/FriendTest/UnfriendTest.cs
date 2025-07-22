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

namespace FamilyFarm.Tests.FriendTest
{
    public class UnfriendTest
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
        [Test]
        public async Task Unfriend_ReturnsOk_WhenUnfriendSuccessful()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var targetId = "682bf676686a672acb6a6380";

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _friendService.Setup(x => x.Unfriend(accId, targetId))
                .ReturnsAsync(true);

            // Act
            var result = await _friendController.Unfriend(targetId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(true, okResult.Value);
        }

        [Test]
        public async Task Unfriend_ReturnsBadRequest_WhenUnfriendFails()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var targetId = "682bf676686a672acb6a6380";

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _friendService.Setup(x => x.Unfriend(accId, null))
                .ReturnsAsync(false);

            // Act
            var result = await _friendController.Unfriend(null);

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
        }
        //unauthorize
        [Test]
        public async Task Unfriend_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null); // simulate missing token

            // Act
            var result = await _friendController.Unfriend("682bf676686a672acb6a6380");

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

    }
}
