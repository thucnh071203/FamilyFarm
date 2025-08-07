using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.GroupTest
{
    public class ResponseRequestJoinGroupTest
    {
        private Mock<IGroupMemberService> _groupMemberServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupMemberController _controller;
        private Mock<ISearchHistoryService> _searchHistoryServiceMock;
        private Mock<IAccountService> _accountServiceMock;

        [SetUp]
        public void Setup()
        {
           _groupMemberServiceMock = new Mock<IGroupMemberService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _searchHistoryServiceMock = new Mock<ISearchHistoryService>();
            _controller = new GroupMemberController(_groupMemberServiceMock.Object, _authServiceMock.Object, 
                _searchHistoryServiceMock.Object, _accountServiceMock.Object);
        }
        [Test]
        public async Task RequestToJoinGroup_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.RequestToJoinGroup("686cd6dff902c8f76207a8dd");

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task RequestToJoinGroup_ReturnsBadRequest_WhenRequestAlreadySentOrMember()
        {
            // Arrange
            var accId = "6884f1546e60165db6b1b5ba";
            var groupId = "68878342681e86c6c9f6f48d";

            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _groupMemberServiceMock.Setup(x => x.RequestToJoinGroupAsync(accId, groupId))
                .ReturnsAsync((GroupMember)null); // Trả về null khi người dùng đã gửi yêu cầu hoặc là thành viên

            // Act
            var result = await _controller.RequestToJoinGroup(groupId);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [Test]
        public async Task RequestToJoinGroup_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var accId = "686c72a8a103667c96bb6000";
            var groupId = "68878342681e86c6c9f6f48d";

            var expectedGroupMember = new GroupMember
            {
                GroupMemberId = "gm001",
                GroupRoleId = "role001",
                GroupId = groupId,
                AccId = accId,
                JointAt = DateTime.UtcNow,
                MemberStatus = "Pending",
                InviteByAccId = null,
                LeftAt = null
            };

            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _groupMemberServiceMock.Setup(x => x.RequestToJoinGroupAsync(accId, groupId))
                .ReturnsAsync(expectedGroupMember);

            // Act
            var result = await _controller.RequestToJoinGroup(groupId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            //dynamic value = okResult.Value;
            //Assert.IsTrue(value.Success);
            //Assert.AreEqual("Send request to group successfuly", value.Message);

            //var data = value.Data as GroupMember;
            //Assert.IsNotNull(data);
            //Assert.AreEqual(expectedGroupMember.GroupMemberId, data.GroupMemberId);
            //Assert.AreEqual(expectedGroupMember.GroupRoleId, data.GroupRoleId);
            //Assert.AreEqual(expectedGroupMember.GroupId, data.GroupId);
            //Assert.AreEqual(expectedGroupMember.AccId, data.AccId);
            //Assert.AreEqual(expectedGroupMember.MemberStatus, data.MemberStatus);
        }

    }
}
