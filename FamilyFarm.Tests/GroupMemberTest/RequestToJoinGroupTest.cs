using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.GroupMemberTest
{
    [TestFixture]
    public class RequestToJoinGroupTests
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
                null!);
        }

        // ✅ TC01: Không có token
        [Test]
        public async Task NoToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.RequestToJoinGroup("group123") as UnauthorizedResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(401, result!.StatusCode);
        }

        // ✅ TC02: Đã gửi yêu cầu trước đó hoặc là thành viên
        [Test]
        public async Task AlreadyRequestedOrMember_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "acc001" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x => x.RequestToJoinGroupAsync("acc001", "group123")).ReturnsAsync((GroupMember?)null);

            var result = await _controller.RequestToJoinGroup("group123") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result!.StatusCode);
            Assert.IsTrue(result.Value?.ToString()?.Contains("already") ?? false);
        }

        // ✅ TC03: Gửi yêu cầu thành công
        [Test]
        public async Task ValidRequest_ShouldReturnOk()
        {
            var accId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();

            var user = new UserClaimsResponseDTO { AccId = accId, RoleName = "Farmer" };
            var mockGroupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                AccId = accId,
                GroupId = groupId,
                GroupRoleId = "role001",
                MemberStatus = "Pending",
                JointAt = DateTime.UtcNow,
                InviteByAccId = "acc002",
                LeftAt = DateTime.UtcNow
            };

            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x => x.RequestToJoinGroupAsync(accId, groupId))
                                   .ReturnsAsync(mockGroupMember);

            var result = await _controller.RequestToJoinGroup(groupId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Cách 1: Pattern matching để đọc anonymous object
            var value = result.Value;
            var successProp = value?.GetType().GetProperty("Success")?.GetValue(value, null);
            var messageProp = value?.GetType().GetProperty("Message")?.GetValue(value, null);
            var dataProp = value?.GetType().GetProperty("Data")?.GetValue(value, null);
            var dataGroupIdProp = dataProp?.GetType().GetProperty("GroupId")?.GetValue(dataProp, null);

            Assert.AreEqual(true, successProp);
            Assert.AreEqual("Send request to group successfuly", messageProp);
            Assert.AreEqual(groupId, dataGroupIdProp);
        }


        // ✅ TC04: Không phải Farmer
        [Test]
        public async Task NonFarmerUser_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "acc001", RoleName = "Admin" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.RequestToJoinGroup("group123") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsTrue(result.Value?.ToString()?.Contains("You send already or you are member.") ?? false);
        }

        // ✅ TC05: Không có booking liên kết (service trả null)
        [Test]
        public async Task NoLinkedBooking_ShouldReturnBadRequest()
        {
            var accId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();
            var user = new UserClaimsResponseDTO { AccId = accId, RoleName = "Farmer" };

            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            // Dù là Farmer, service trả null (vì thiếu booking liên kết)
            _groupMemberServiceMock.Setup(x => x.RequestToJoinGroupAsync(accId, groupId))
                                   .ReturnsAsync((GroupMember?)null);

            var result = await _controller.RequestToJoinGroup(groupId) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsTrue(result.Value?.ToString()?.Contains("already") ?? false); // hoặc thông báo khác tùy logic
        }

        // ✅ TC06: GroupId không hợp lệ (không phải ObjectId)
        [Test]
        public async Task InvalidGroupId_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = ObjectId.GenerateNewId().ToString(), RoleName = "Farmer" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var invalidGroupId = "not-a-valid-object-id";

            var result = await _controller.RequestToJoinGroup(invalidGroupId) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsTrue(result.Value?.ToString()?.Contains("You send already or you are member.") ?? false);
        }


    }
}
