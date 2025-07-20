using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
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
    [TestFixture]
    public class ListFriendTest
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
        public async Task GetListFriends_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _friendController.GetListFriends();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }

        [Test]
        public async Task GetListFriends_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            //giả lập như user đã đăng nhập rồi.
            _authenticationService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "6843e30d3c4871a0339bb1a9" });

            //giả lập trường hợp lỗi hoặc không có dữ liệu
            _friendService.Setup(f => f.GetListFriends("6843e30d3c4871a0339bb1a9"))
                .ReturnsAsync((FriendResponseDTO)null);

            var result = await _friendController.GetListFriends();

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task GetListFriends_ReturnsNotFound_WhenIsSuccessFalse()
        {
            _authenticationService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "6843e30d3c4871a0339bb1a9" });

            var failedResponse = new FriendResponseDTO
            {
                IsSuccess = false,
                Message = "Not found",
                Count = 0,
                Data = null
            };

            _friendService.Setup(f => f.GetListFriends("6843e30d3c4871a0339bb1a9"))
                .ReturnsAsync(failedResponse);

            var result = await _friendController.GetListFriends();

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task GetListFriends_ReturnsOk_WhenSuccessful()
        {
            var mockFriends = new List<FriendMapper>
    {
        new FriendMapper
        {
            AccId = "f1",
            Username = "john123",
            FullName = "John Doe",
            Birthday = new DateTime(1995, 5, 20),
            Gender = "Male",
            City = "Hanoi",
            Country = "Vietnam",
            Address = "123 Main St",
            Avatar = "avatar1.jpg",
            Background = "bg1.jpg",
            Certificate = "CertA",
            WorkAt = "CompanyX",
            StudyAt = "UniversityY",
            Status = 1,
            FriendStatus = "Friend",
            MutualFriend = 5
        },
        new FriendMapper
        {
            AccId = "f2",
            Username = "jane456",
            FullName = "Jane Smith",
            Birthday = new DateTime(1998, 8, 15),
            Gender = "Female",
            City = "Ho Chi Minh City",
            Country = "Vietnam",
            Address = "456 Central Rd",
            Avatar = "avatar2.jpg",
            Background = "bg2.jpg",
            Certificate = "CertB",
            WorkAt = "CompanyY",
            StudyAt = "UniversityZ",
            Status = 1,
            FriendStatus = "Friend",
            MutualFriend = 2
        }
    };

            var response = new FriendResponseDTO
            {
                IsSuccess = true,
                Message = "Lấy danh sách bạn thành công",
                Count = mockFriends.Count,
                Data = mockFriends
            };

            _authenticationService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "6843e30d3c4871a0339bb1a9" });

            _friendService.Setup(f => f.GetListFriends("6843e30d3c4871a0339bb1a9")).ReturnsAsync(response);

            var result = await _friendController.GetListFriends();

            var okResult = result.Result as OkObjectResult;

            Assert.IsNotNull(okResult);
            var returnedResponse = okResult.Value as FriendResponseDTO;
            Assert.IsTrue(returnedResponse.IsSuccess);
            Assert.AreEqual(2, returnedResponse.Count);

            // Check specific properties
            Assert.AreEqual("john123", returnedResponse.Data[0].Username);
            Assert.AreEqual("Jane Smith", returnedResponse.Data[1].FullName);
            Assert.AreEqual("Vietnam", returnedResponse.Data[1].Country);
            Assert.AreEqual("Friend", returnedResponse.Data[0].FriendStatus);
        }



    }
}
