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
    public class ResponseFriendRequestTest
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
        public async Task AcceptFriendRequest_ReturnsOk_WhenAcceptedSuccessfully()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var otherId = "682bf676686a672acb6a6380";

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _friendRequestService.Setup(f => f.AcceptFriendRequestAsync(accId, otherId))
                .ReturnsAsync(true);

            // Act
            var result = await _friendController.AcceptFriendRequest(otherId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Friend request accepted.", okResult.Value);
        }

        [Test]
        public async Task AcceptFriendRequest_ReturnsBadRequest_WhenAcceptFails()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var otherId = "682bf676686a672acb6a6380";

            _authenticationService.Setup(x => x.GetDataFromToken())
      .Returns(new UserClaimsResponseDTO { AccId = accId });

            _friendRequestService.Setup(f => f.AcceptFriendRequestAsync(accId, null))
                .ReturnsAsync(false);

            // Act
            var result = await _friendController.AcceptFriendRequest(null);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Friend request could not be accepted.", badRequestResult.Value);
        }

        [Test]
        public async Task AcceptFriendRequest_ReturnsUnauthorized_WhenAccIdIsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
     .Returns((UserClaimsResponseDTO)null); // No user

            // Act
            var result = await _friendController.AcceptFriendRequest("682bf676686a672acb6a6380");

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

    }
}
