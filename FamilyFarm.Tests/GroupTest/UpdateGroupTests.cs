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
    class UpdateGroupTests
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
        public async Task UpdateGroup_ValidRequest_ShouldReturnSuccess()
        {
            var request = new GroupRequestDTO
            {
                GroupName = "Người yêu lúa",
                PrivacyType = "Public",
                GroupAvatar = Mock.Of<IFormFile>(),
                GroupBackground = Mock.Of<IFormFile>()
            };
            var accId = "64aeb3f8c2bd3f00124c15e1";
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group updated successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, accId) }
            });

            var result = await _controller.UpdateGroup("gid123", request);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
        }

        [Test]
        public async Task UpdateGroup_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var dummyRequest = new GroupRequestDTO
            {
                GroupName = "dummy",
                PrivacyType = "dummy"
            };

            var result = await _controller.UpdateGroup("gid123", dummyRequest);

            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);
        }

        [Test]
        public async Task UpdateGroup_EmptyGroupName_ShouldReturnBadRequest()
        {
            var request = new GroupRequestDTO { GroupName = "", PrivacyType = "Public" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });
            var result = await _controller.UpdateGroup("gid123", request);
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
        }

        [Test]
        public async Task UpdateGroup_InvalidAvatarUpload_ShouldReturnFail()
        {
            var request = new GroupRequestDTO { GroupName = "Name", PrivacyType = "Public", GroupAvatar = Mock.Of<IFormFile>() };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });

            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Upload fail"
            });

            var result = await _controller.UpdateGroup("gid123", request);
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
        }

        [Test]
        public async Task UpdateGroup_InvalidBackgroundUpload_ShouldReturnFail()
        {
            var request = new GroupRequestDTO { GroupName = "Name", PrivacyType = "Public", GroupBackground = Mock.Of<IFormFile>() };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });

            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Upload fail"
            });

            var result = await _controller.UpdateGroup("gid123", request);
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
        }

        [Test]
        public async Task UpdateGroup_NullAvatar_ShouldStillSucceed()
        {
            var request = new GroupRequestDTO { GroupName = "Test", PrivacyType = "Public", GroupAvatar = null };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });
            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group updated successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, "64aeb3f8c2bd3f00124c15e1") }
            });

            var result = await _controller.UpdateGroup("gid123", request);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateGroup_NullBackground_ShouldStillSucceed()
        {
            var request = new GroupRequestDTO { GroupName = "Test", PrivacyType = "Public", GroupBackground = null };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });
            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group updated successfully",
                Data = new List<Group> { CreateMockGroup("gid123", request.GroupName, "64aeb3f8c2bd3f00124c15e1") }
            });

            var result = await _controller.UpdateGroup("gid123", request);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateGroup_NotOwner_ShouldReturnFail()
        {
            var request = new GroupRequestDTO { GroupName = "Test", PrivacyType = "Public" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" });
            _groupServiceMock.Setup(x => x.UpdateGroup("gid123", It.IsAny<GroupRequestDTO>())).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Provider does not match"
            });

            var result = await _controller.UpdateGroup("gid123", request);
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
        }
    }
}
