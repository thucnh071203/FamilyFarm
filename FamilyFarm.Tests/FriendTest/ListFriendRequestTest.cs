using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
    public class ListFriendRequestTest
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
        public async Task GetReceiveRequest_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _friendController.GetReceiveRequest();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }
        [Test]
        public async Task GetReceiveRequest_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "xuanbui" });

            _friendRequestService.Setup(f => f.GetAllReceiveFriendRequests("xuanbui"))
                .ReturnsAsync((FriendResponseDTO)null);

            // Act
            var result = await _friendController.GetReceiveRequest();

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task GetReceiveRequest_ReturnsNotFound_WhenIsSuccessIsFalse()
        {
            var response = new FriendResponseDTO
            {
                IsSuccess = false,
                Message = "No requests found"
            };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "xuanbui" });

            _friendRequestService.Setup(f => f.GetAllReceiveFriendRequests("xuanbui"))
                .ReturnsAsync(response);

            var result = await _friendController.GetReceiveRequest();

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task GetReceiveRequest_ReturnsOk_WhenSuccessful()
        {
            var response = new FriendResponseDTO
            {
                IsSuccess = true,
                Data = new List<FriendMapper>() // giả định có dữ liệu
            };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "xuanbui" });

            _friendRequestService.Setup(f => f.GetAllReceiveFriendRequests("xuanbui"))
                .ReturnsAsync(response);

            var result = await _friendController.GetReceiveRequest();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

    }
}
