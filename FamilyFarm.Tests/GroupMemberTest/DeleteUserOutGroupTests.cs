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
    public class DeleteUserOutGroupTests
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
            GroupMemberId = "680d28bcb2560a3fdd73707c",
            GroupRoleId = "memberroleid",
            GroupId = "gid123",
            AccId = "acc456",
            JointAt = DateTime.UtcNow,
            MemberStatus = "Active",
            InviteByAccId = "acc123",
            LeftAt = null
        };

        [Test]
        public async Task DeleteMember_ValidRequest_ShouldReturnSuccess()
        {

            var groupMemberId = "680d28bcb2560a3fdd73707c";

            _authServiceMock
                .Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "680d28bcb2560a3fdd73707c" });

            _groupMemberServiceMock
                .Setup(x => x.GetGroupMemberById(groupMemberId))
                .ReturnsAsync(GetMockMember());

            _groupMemberServiceMock
                .Setup(x => x.DeleteGroupMember(groupMemberId))
                .ReturnsAsync(1);

            var result = await _controller.DeleteGroupMember(groupMemberId) as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);
                Assert.AreEqual("Delete successfully!", result.Value);
            });
        }

        [Test]
        public async Task DeleteMember_NoTokenRequired_ShouldPass() // Do controller không kiểm tra token nên không lỗi
        {
            _authServiceMock
               .Setup(x => x.GetDataFromToken())
               .Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.DeleteGroupMember("680d28bcb2560a3fdd73707c") as UnauthorizedObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(401, result.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", result.Value);
            });
        }

        [Test]
        public async Task DeleteMember_EmptyId_ShouldReturnBadRequest()
        {
            _authServiceMock
                .Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "acc123" });

            _groupMemberServiceMock
                .Setup(x => x.GetGroupMemberById(""))
                .ReturnsAsync((GroupMember?)null);

            var result = await _controller.DeleteGroupMember("") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                Assert.AreEqual("Invalid AccIds.", result.Value);
            });
        }
    }
}
