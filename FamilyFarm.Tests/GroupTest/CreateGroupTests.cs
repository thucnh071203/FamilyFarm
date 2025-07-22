using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.GroupTest
{
    [TestFixture]
    public class CreateGroupTests
    {
        private Mock<IGroupService> _groupServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupController _controller;

        private Group CreateMockGroup(string id, string name, string ownerId)
        {
            return new Group
            {
                GroupId = id,
                GroupName = name,
                GroupAvatar = "url-avatar",
                GroupBackground = "url-background",
                PrivacyType = "Public",
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            };
        }

        [SetUp]
        public void Setup()
        {
            _groupServiceMock = new Mock<IGroupService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new GroupController(_groupServiceMock.Object, _authServiceMock.Object);
        }

        [Test]
        public async Task CreateGroup_ValidRequest_ReturnsSuccess()
        {
            var request = new GroupRequestDTO
            {
                GroupName = "Người yêu lúa",
                PrivacyType = "Public",
                GroupAvatar = Mock.Of<IFormFile>(),
                GroupBackground = Mock.Of<IFormFile>()
            };

            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1", Username = "tester" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, user.AccId!) }
            });

            var result = await _controller.CreateGroup(request);
            var okResult = result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(okResult);
                Assert.AreEqual(200, okResult.StatusCode);
                var data = okResult.Value as GroupResponseDTO;
                Assert.IsTrue(data?.Success);
                Assert.AreEqual("Group created successfully", data?.Message);
            });
        }

        [Test]
        public async Task CreateGroup_MissingToken_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public" };
            var result = await _controller.CreateGroup(request);

            var unauthorized = result as UnauthorizedObjectResult;
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(unauthorized);
                Assert.AreEqual(401, unauthorized.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", unauthorized.Value);
            });
        }

        [Test]
        public async Task CreateGroup_EmptyGroupName_ReturnsBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123" });

            var request = new GroupRequestDTO { GroupName = "", PrivacyType = "Public" };
            var result = await _controller.CreateGroup(request);

            var badRequest = result as BadRequestObjectResult;
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(badRequest);
                Assert.AreEqual(400, badRequest.StatusCode);
                Assert.AreEqual("GroupName and PrivacyType must not be empty.", badRequest.Value);
            });
        }

        [Test]
        public async Task CreateGroup_InvalidAvatarUpload_ShouldReturnServiceFail()
        {
            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public", GroupAvatar = Mock.Of<IFormFile>() };
            var user = new UserClaimsResponseDTO { AccId = "acc123" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Avatar upload failed"
            });

            var result = await _controller.CreateGroup(request);
            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [Test]
        public async Task CreateGroup_InvalidBackgroundUpload_ShouldReturnServiceFail()
        {
            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public", GroupBackground = Mock.Of<IFormFile>() };
            var user = new UserClaimsResponseDTO { AccId = "acc123" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Background upload failed"
            });

            var result = await _controller.CreateGroup(request);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [Test]
        public async Task CreateGroup_UserHasNoFriends_StillCreateGroupSuccessfully()
        {
            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public" };
            var user = new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, user.AccId!) }
            });

            var result = await _controller.CreateGroup(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task CreateGroup_NullAvatar_ShouldSucceed()
        {
            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public", GroupAvatar = null };
            var user = new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, user.AccId!) }
            });

            var result = await _controller.CreateGroup(request);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateGroup_NullBackground_ShouldSucceed()
        {
            var request = new GroupRequestDTO { GroupName = "Người yêu lúa", PrivacyType = "Public", GroupBackground = null };
            var user = new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, user.AccId!) }
            });

            var result = await _controller.CreateGroup(request);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateGroup_HasFriendsButNoInvites_ShouldSucceedWithoutInviting()
        {
            var request = new GroupRequestDTO
            {
                GroupName = "Tôi tạo nhóm nhưng không mời bạn",
                PrivacyType = "Public"
            };

            var user = new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.CreateGroup(It.IsAny<GroupRequestDTO>()))
                .ReturnsAsync(new GroupResponseDTO
                {
                    Success = true,
                    Message = "Group created successfully",
                    Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, user.AccId!) }
                });

            var result = await _controller.CreateGroup(request);
            var ok = result as OkObjectResult;

            Assert.IsNotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
        }
    }
}
