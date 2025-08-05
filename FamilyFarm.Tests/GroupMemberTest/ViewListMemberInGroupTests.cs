using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.GroupMemberTest
{
    [TestFixture]
    public class ViewListMemberInGroupTests
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

        private List<GroupMemberResponseDTO> GetMockMembers() => new()
        {
            new GroupMemberResponseDTO
            {
                GroupMemberId = "mem001",
                GroupId = "680b79302b2b9f616df01554",
                AccId = "6808484b0849665c281db8b9",
                JointAt = DateTime.UtcNow,
                MemberStatus = "Active",
                FullName = "Nguyen Van A",
                Avatar = "url-to-avatar",
                City = "Hanoi",
                RoleInGroupId = "role123"
            },
            new GroupMemberResponseDTO
            {
                GroupMemberId = "mem002",
                GroupId = "680b79302b2b9f616df01554",
                AccId = "682160d42663032aed86ff92",
                JointAt = DateTime.UtcNow,
                MemberStatus = "Active",
                FullName = "Le Thi B",
                Avatar = "url-to-avatar-2",
                City = "Ho Chi Minh",
                RoleInGroupId = "role124"
            }
        };


        // Valid request, user is in group, return member list
        [Test]
        public async Task ViewMembers_ValidRequest_ReturnsList()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808484b0849665c281db8b9" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x =>
                x.GetUsersInGroupAsync("680b79302b2b9f616df01554", user.AccId!))
                .ReturnsAsync(GetMockMembers());

            var result = await _controller.GetUsersByGroupId("680b79302b2b9f616df01554") as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var members = result.Value as List<GroupMemberResponseDTO>;
                Assert.IsNotNull(members);
                Assert.AreEqual(2, members!.Count);
            });
        }

        // Request without token
        [Test]
        public async Task ViewMembers_NoToken_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.GetUsersByGroupId("680b79302b2b9f616df01554");

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("Invalid token or user not found.", unauthorized?.Value);
        }

        //  User not in group, throw UnauthorizedAccessException
        [Test]
        public void ViewMembers_UserNotInGroup_ShouldThrow()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808484b0849665c281db8b9" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x =>
                x.GetUsersInGroupAsync("680b79302b2b9f616df01554", user.AccId!))
                .ThrowsAsync(new UnauthorizedAccessException("You are not a member of this group."));

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _controller.GetUsersByGroupId("680b79302b2b9f616df01554"));

            Assert.AreEqual("You are not a member of this group.", ex?.Message);
        }

        //  Group has no members (empty list)
        [Test]
        public async Task ViewMembers_GroupHasNoMember_ShouldReturnNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808484b0849665c281db8b9" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x =>
                x.GetUsersInGroupAsync("680b79302b2b9f616df01554", user.AccId!))
                .ReturnsAsync(new List<GroupMemberResponseDTO>());

            var result = await _controller.GetUsersByGroupId("680b79302b2b9f616df01554");

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("No users found in this group.", notFound?.Value);
        }


        // GroupId is empty string
        [Test]
        public void ViewMembers_EmptyGroupId_ShouldThrowException()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808484b0849665c281db8b9" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _groupMemberServiceMock
                .Setup(x => x.GetUsersInGroupAsync("", user.AccId!))
                .ThrowsAsync(new ArgumentNullException("groupId", "GroupId is required"));

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _controller.GetUsersByGroupId(""));
        }

    }
}
