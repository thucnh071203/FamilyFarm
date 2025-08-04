using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.GroupMemberTest
{
    [TestFixture]
    public class ListRequestToJoinGroup
    {
        private Mock<IGroupMemberService> _groupMemberServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupMemberController _controller;

        [SetUp]
        public void Setup()
        {
            _groupMemberServiceMock = new Mock<IGroupMemberService>();
            _authServiceMock = new Mock<IAuthenticationService>();

            _controller = new GroupMemberController(
                _groupMemberServiceMock.Object,
                _authServiceMock.Object,
                Mock.Of<ISearchHistoryService>(),
                null!
            );
        }

        [Test]
        public async Task NoToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken())
                            .Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.GetJoinRequests("660000000000000000000001");

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
            Assert.AreEqual(401, ((UnauthorizedResult)result).StatusCode);
        }

        [Test]
        public async Task GroupIdIsEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken())
                            .Returns(new UserClaimsResponseDTO { AccId = "acc001" });

            // Act
            var result = await _controller.GetJoinRequests("") as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);

            var value = result.Value as GroupMemberListResponse;
            Assert.IsFalse(value!.Success);
            Assert.AreEqual("GroupId is required", value.Message);
            Assert.IsNull(value.Data);
        }

        [Test]
        public async Task GroupHasNoJoinRequests_ShouldReturnNotFound()
        {
            var groupId = ObjectId.GenerateNewId().ToString();

            _authServiceMock.Setup(x => x.GetDataFromToken())
                            .Returns(new UserClaimsResponseDTO { AccId = "acc001" });

            _groupMemberServiceMock.Setup(x => x.GetJoinRequestsAsync(groupId))
                                   .ReturnsAsync(new List<GroupMemberRequest>());

            var result = await _controller.GetJoinRequests(groupId) as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);

            var value = result.Value as GroupMemberListResponse;
            Assert.IsFalse(value!.Success);
            Assert.AreEqual("No join requests found for this group.", value.Message);
            Assert.IsNull(value.Data);
        }

        [Test]
        public async Task GroupHasJoinRequests_ShouldReturnOkWithList()
        {
            var groupId = ObjectId.GenerateNewId().ToString();

            _authServiceMock.Setup(x => x.GetDataFromToken())
                            .Returns(new UserClaimsResponseDTO { AccId = "acc001" });

            var mockRequests = new List<GroupMemberRequest>
        {
            new GroupMemberRequest
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                GroupId = groupId,
                AccId = "acc001",
                JointAt = DateTime.UtcNow,
                MemberStatus = "Pending",
                InviteByAccId = "admin001",
                AccountFullName = "Nguyen Van A",
                AccountAvatar = "avatar.jpg",
                City = "HCM"
            }
        };

            _groupMemberServiceMock.Setup(x => x.GetJoinRequestsAsync(groupId))
                                   .ReturnsAsync(mockRequests);

            var result = await _controller.GetJoinRequests(groupId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var value = result.Value as GroupMemberListResponse;
            Assert.IsTrue(value!.Success);
            Assert.AreEqual("Get list join requests successfully.", value.Message);
            Assert.IsNotNull(value.Data);
            Assert.AreEqual(1, value.Data.Count);
            Assert.AreEqual("acc001", value.Data[0].AccId);
        }
    }

}
