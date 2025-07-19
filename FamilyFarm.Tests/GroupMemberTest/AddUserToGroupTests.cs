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

namespace FamilyFarm.Tests.GroupMemberTest
{
    [TestFixture]
    class AddUserToGroupTests
    {
        private Mock<IGroupMemberService> _groupMemberServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupMemberController _controller;

        [SetUp]
        public void Setup()
        {
            _groupMemberServiceMock = new Mock<IGroupMemberService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new GroupMemberController(_groupMemberServiceMock.Object, _authServiceMock.Object, null!, null!);
        }

        private GroupMember GetMockMember() => new GroupMember
        {
            GroupMemberId = "member123",
            GroupRoleId = "role123",
            GroupId = "gid123",
            AccId = "acc456",
            JointAt = DateTime.UtcNow,
            MemberStatus = "Active",
            InviteByAccId = "acc123",
            LeftAt = null
        };

        [Test]
        public async Task AddMember_ValidRequest_ShouldReturnSuccess()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock
                .Setup(x => x.AddGroupMember("gid123", "acc456", "64aeb3f8c2bd3f00124c15e1"))
                .ReturnsAsync(GetMockMember());

            var result = await _controller.AddGroupMember("gid123", "acc456") as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);

                var msg = result.Value?.GetType().GetProperty("Message")?.GetValue(result.Value, null);
                Assert.AreEqual("Add member successfully.", msg?.ToString());
            });
        }


        [Test]
        public async Task AddMember_NoToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.AddGroupMember("gid123", "64aeb3f8c2bd3f00124c1536") as UnauthorizedObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(401, result.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", result.Value);
            });
        }

        [Test]
        public async Task AddMember_RequestWithEmptyIds_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "64aeb3f8c2bd3f00124c15e1" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.AddGroupMember("gid123", "") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);

                var val = result.Value?.GetType().GetProperty("Message")?.GetValue(result.Value, null);
                Assert.AreEqual("GroupId or AccountId is null", val?.ToString());
            });
        }
    }
}
