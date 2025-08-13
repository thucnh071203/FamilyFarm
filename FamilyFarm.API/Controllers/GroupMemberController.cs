using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyFarm.API.Controllers
{
    [Route("api/group-member")]
    [ApiController]
    public class GroupMemberController : ControllerBase
    {
        private readonly IGroupMemberService _groupMemberService;
        private readonly IAuthenticationService _authenService;
        private readonly ISearchHistoryService _searchHistoryService;
        private readonly IAccountService _accountService;

        public GroupMemberController(IGroupMemberService groupMemberService, IAuthenticationService authenService, ISearchHistoryService searchHistoryService, IAccountService accountService)
        {
            _groupMemberService = groupMemberService;
            _authenService = authenService;
            _searchHistoryService = searchHistoryService;
            _accountService = accountService;
        }

        [HttpGet("get-by-id/{groupMemberId}")]
        public async Task<IActionResult> GetGroupMemberById(string groupMemberId)
        {
            var groupMember = await _groupMemberService.GetGroupMemberById(groupMemberId);
            return Ok(groupMemberId);
        }

        [HttpPost("create/{groupId}/{accountId}")]
        [Authorize]
        public async Task<IActionResult> AddGroupMember(string groupId, string accountId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            string inviterId = account.AccId;

            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(accountId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "GroupId or AccountId is null",
                });
            }

            var result = await _groupMemberService.AddGroupMember(groupId, accountId, inviterId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Add member failed."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Add member successfully.",
                Data = result
            });
        }

        [HttpDelete("delete/{groupMemberId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGroupMember(string groupMemberId)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(userClaims.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var group = await _groupMemberService.GetGroupMemberById(groupMemberId);
            if (group == null)
                return BadRequest("Group member not found");

            await _groupMemberService.DeleteGroupMember(groupMemberId, userClaims.AccId);
            return Ok("Delete successfully!");
        }

        [HttpGet("users/in-group/{groupId}")]
        [Authorize]
        public async Task<IActionResult> GetUsersByGroupId(string groupId)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return Unauthorized("Invalid token or user not found.");

            var users = await _groupMemberService.GetUsersInGroupAsync(groupId, userClaims.AccId);
            if (users == null || users.Count == 0)
                return NotFound("No users found in this group.");

            return Ok(users);
        }

        [HttpGet("search-user-in-group/{groupId}")]
        public async Task<IActionResult> SearchUsersInGroup(string groupId, [FromQuery] string keyword)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var users = await _groupMemberService.SearchUsersInGroupAsync(groupId, keyword);
            var search = await _searchHistoryService.AddSearchHistory(accId, keyword);

            if (users.Count == 0)
                return NotFound("Not found members.");

            return Ok(users);
        }

        //[HttpGet("list-request-to-join/{groupId}")]
        //public async Task<IActionResult> GetJoinRequests(string groupId)
        //{
        //    if (string.IsNullOrEmpty(groupId))
        //    {
        //        return BadRequest(new GroupMemberListResponse
        //        {
        //            Success = false,
        //            Message = "GroupId is required",
        //            Data = null
        //        });
        //    }

        //    var data = await _groupMemberService.GetJoinRequestsAsync(groupId);


        //    if (data == null || data.Count == 0)
        //    {
        //        return NotFound(new GroupMemberListResponse
        //        {
        //            Success = false,
        //            Message = "No join requests found for this group.",
        //            Data = null
        //        });
        //    }

        //    return Ok(new GroupMemberListResponse
        //    {
        //        Success = true,
        //        Message = "Get list join requests successfully.",
        //        Data = data
        //    });
        //}

        [HttpGet("list-request-to-join/{groupId}")]
        [Authorize]
        public async Task<IActionResult> GetJoinRequests(string groupId)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return Unauthorized(); // 🟢 Giải quyết được lỗi test

            if (string.IsNullOrEmpty(groupId))
            {
                return BadRequest(new GroupMemberListResponse
                {
                    Success = false,
                    Message = "GroupId is required",
                    Data = null
                });
            }

            var data = await _groupMemberService.GetJoinRequestsAsync(groupId);

            if (data == null || data.Count == 0)
            {
                return NotFound(new GroupMemberListResponse
                {
                    Success = false,
                    Message = "No join requests found for this group.",
                    Data = null
                });
            }

            return Ok(new GroupMemberListResponse
            {
                Success = true,
                Message = "Get list join requests successfully.",
                Data = data
            });
        }

        [HttpPost("request-to-join/{groupId}")]
        [Authorize]
        public async Task<IActionResult> RequestToJoinGroup(string groupId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null) return Unauthorized();
            var result = await _groupMemberService.RequestToJoinGroupAsync(accId, groupId);
            //if (result == null)
            //    return BadRequest(new { Success = false, Message = "You send already or you are member." });

            //return Ok(new { Success = true, Message = "Send request to group successfuly", Data = result });
            if (result == null)
                return BadRequest(new { Success = false, Message = "You send already or you are member." });

            return Ok(new { Success = true, Message = "Send request to group successfully", Data = result });
        }

        [HttpPut("response-to-join-group/{groupMemberId}")]
        public async Task<IActionResult> RespondToJoinRequest(string groupMemberId, [FromQuery] string status)
        {
            var success = await _groupMemberService.RespondToJoinRequestAsync(groupMemberId, status);

            if (!success)
            {
                return BadRequest(new { message = "Invalid request or status" });
            }

            return Ok(new { message = $"Join request has been {status.ToLower()}ed successfully" });
        }
        [HttpPut("update-role")]
        [Authorize]
        public async Task<IActionResult> UpdateMemberRole([FromQuery] string groupMemberId, [FromQuery] string newGroupRoleId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null) return Unauthorized();
            var result = await _groupMemberService.UpdateMemberRoleAsync(groupMemberId, newGroupRoleId);
            if (result)
                return Ok("Role updated successfully.");
            return NotFound("Member not found or update failed.");
        }

        [HttpGet("get-a-user-in-group/{groupId}")]
        [Authorize]
        public async Task<IActionResult> GetAUserInGroup(string groupId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var user = await _groupMemberService.GetOneUserInGroupAsync(groupId, account.AccId);
            if (user == null)
                return NotFound("No user found in this group.");

            return Ok(user);
        }
        [HttpDelete("leave/{groupId}")]
        public async Task<IActionResult> LeaveGroup(string groupId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var check = await _groupMemberService.LeaveGroupAsync(groupId, accId);
           
            return Ok(check);
        }
        [HttpPost("invite/{groupId}/{accountId}")]
        [Authorize]
        public async Task<IActionResult> InviteGroupMember(string groupId, string accountId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            string inviterId = account.AccId;

            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(accountId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "GroupId or AccountId is null",
                });
            }

            var result = await _groupMemberService.InviteGroupMember(groupId, accountId, inviterId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Invite member failed."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Invite member successfully.",
                Data = result
            });
        }

        [HttpPut("response-to-invite-group/{groupMemberId}")]
        public async Task<IActionResult> RespondToInviteRequest(string groupMemberId, [FromQuery] string status)
        {
            //var success = await _groupMemberService.RespondToInviteRequestAsync(groupMemberId, status);

            //if (!success)
            //{
            //    return BadRequest(new { message = "Invalid request or status" });
            //}

            var (success, message) = await _groupMemberService.RespondToInviteRequestAsync(groupMemberId, status);

            if (!success)
            {
                // Nếu service đã nói rõ là group không tồn tại → 404
                if (string.Equals(message, "The group you respond has unavailable", StringComparison.Ordinal))
                    return NotFound(new { message });

                // Nếu invite không còn tồn tại → 404 cũng hợp lý
                if (string.Equals(message, "The invite request no longer exists", StringComparison.Ordinal))
                    return NotFound(new { message });

                // Các lỗi còn lại → 400
                return BadRequest(new { message = message ?? "Invalid request or status" });
            }

            return Ok(new { message = $"Invite request has been {status.ToLower()}ed successfully" });
        }

        [HttpGet("get-by-groupid-accid/{groupId}/{accountId}")]
        [Authorize]
        public async Task<IActionResult> GetMemberInvitedOrJoinedGroupAsync(string groupId, string accountId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var groupMember = await _groupMemberService.GetMemberInvitedOrJoinedGroup(groupId, accountId);

            if (groupMember == null)
                return NotFound("Group member not found or not invited/accepted.");

            return Ok(groupMember);
        }

        [HttpGet("get-member-invite-by-id/{groupMemberId}")]
        public async Task<IActionResult> GetGroupMemberInviteById(string groupMemberId)
        {
            var groupMember = await _groupMemberService.GetGroupMemberInviteById(groupMemberId);

            if (groupMember == null)
                return NotFound("Không tìm thấy lời mời tham gia nhóm với ID đã cho.");

            return Ok(groupMember);
        }
    }
}
