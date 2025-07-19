using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.GroupTest
{
    [TestFixture]
    public class DeleteGroupTests
    {
        private Mock<IGroupService> _groupServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupController _controller;

        [SetUp]
        public void Setup()
        {
            _groupServiceMock = new Mock<IGroupService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new GroupController(_groupServiceMock.Object, _authServiceMock.Object);
        }

        private Group GetMockGroup() => new Group
        {
            GroupId = "gid123",
            GroupName = "Test Group",
            GroupAvatar = "avatar-url",
            GroupBackground = "bg-url",
            PrivacyType = "Public",
            OwnerId = "acc123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            DeletedAt = null,
            IsDeleted = false
        };

        [Test]
        public async Task DeleteGroup_ValidRequest_ReturnsSuccess()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.GetGroupById("gid123")).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Data = new List<Group> { GetMockGroup() }
            });

            _groupServiceMock.Setup(x => x.DeleteGroup("gid123")).ReturnsAsync(new GroupResponseDTO
            {
                Success = true,
                Message = "Group deleted successfully"
            });

            var result = await _controller.DeleteGroup("gid123") as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);
                var response = result.Value as GroupResponseDTO;
                Assert.IsTrue(response!.Success);
            });
        }

        [Test]
        public async Task DeleteGroup_MissingToken_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.DeleteGroup("gid123") as UnauthorizedObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(401, result.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", result.Value);
            });
        }

        [Test]
        public async Task DeleteGroup_NotOwner_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e2" }; // Không phải chủ group
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.DeleteGroup("gid123")).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Provider does not match"
            });

            var result = await _controller.DeleteGroup("gid123") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                var response = result.Value as GroupResponseDTO;
                Assert.IsNotNull(response);
                Assert.AreEqual("Provider does not match", response!.Message);
            });
        }


        [Test]
        public async Task DeleteGroup_GroupNotFound_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.DeleteGroup("gid123")).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Group not found",
                Data = null
            });

            var result = await _controller.DeleteGroup("gid123") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                var response = result.Value as GroupResponseDTO;
                Assert.IsNotNull(response);
                Assert.AreEqual("Group not found", response!.Message);
            });
        }


        [Test]
        public async Task DeleteGroup_WrongConfirmationCode_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupServiceMock.Setup(x => x.DeleteGroup("gid123")).ReturnsAsync(new GroupResponseDTO
            {
                Success = false,
                Message = "Invalid confirmation code"
            });

            var result = await _controller.DeleteGroup("gid123") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                var response = result.Value as GroupResponseDTO;
                Assert.IsNotNull(response);
                Assert.AreEqual("Invalid confirmation code", response!.Message);
            });
        }
    }
}
