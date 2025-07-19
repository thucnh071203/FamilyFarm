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
    class ViewListGroupTests
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

        [Test]
        public async Task TC01_GetAllGroup_WithData_ReturnsSuccess()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab", Username = "testuser" });

            var expected = new GroupResponseDTO
            {
                Success = true,
                Message = "Get all group successfully",
                Count = 2,
                Data = new List<Group>
                {
                    new Group
                    {
                        GroupId = "1",
                        GroupName = "Group 1",
                        GroupAvatar = "url1",
                        GroupBackground = "bg1",
                        PrivacyType = "Public",
                        OwnerId = "owner1",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        DeletedAt = null,
                        IsDeleted = false
                    },
                    new Group
                    {
                        GroupId = "2",
                        GroupName = "Group 2",
                        GroupAvatar = "url2",
                        GroupBackground = "bg2",
                        PrivacyType = "Private",
                        OwnerId = "owner2",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        DeletedAt = null,
                        IsDeleted = false
                    }
                }
            };

            _groupServiceMock.Setup(s => s.GetAllGroup()).ReturnsAsync(expected);

            var result = await _controller.GetAllGroup();
            var okResult = result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(okResult);
                Assert.AreEqual(200, okResult.StatusCode);
                Assert.IsInstanceOf<GroupResponseDTO>(okResult.Value);
                var response = (GroupResponseDTO)okResult.Value!;
                Assert.IsTrue(response.Success);
                Assert.AreEqual(2, response.Count);
            });
        }

        [Test]
        public async Task TC02_GetAllGroup_EmptyList_ReturnsFailureMessage()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "64b1c456a1a2b123456789ab", Username = "testuser" });

            var expected = new GroupResponseDTO
            {
                Success = false,
                Message = "Group list is empty",
                Count = 0,
                Data = new List<Group>()
            };

            _groupServiceMock.Setup(s => s.GetAllGroup()).ReturnsAsync(expected);

            var result = await _controller.GetAllGroup();
            var okResult = result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(okResult);
                Assert.AreEqual(200, okResult.StatusCode);
                var response = (GroupResponseDTO)okResult.Value!;
                Assert.IsFalse(response.Success);
                Assert.AreEqual(0, response.Count);
                Assert.AreEqual("Group list is empty", response.Message);
            });
        }

        [Test]
        public async Task TC03_GetAllGroup_InvalidToken_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.GetAllGroup();

            var unauthorized = result as UnauthorizedObjectResult;
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(unauthorized);
                Assert.AreEqual(401, unauthorized.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", unauthorized.Value);
            });
        }
    }
}
