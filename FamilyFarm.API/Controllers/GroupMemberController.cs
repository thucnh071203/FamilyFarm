using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/group-member")]
    [ApiController]
    public class GroupMemberController : ControllerBase
    {
        private readonly IGroupMemberService _groupMemberService;

        public GroupMemberController(IGroupMemberService groupMemberService)
        {
            _groupMemberService = groupMemberService;
        }

        [HttpGet("get-by-id/{groupMemberId}")]
        public async Task<IActionResult> GetGroupMemberById(string groupMemberId)
        {
            var groupMember = await _groupMemberService.GetGroupMemberById(groupMemberId);
            return Ok(groupMemberId);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddGroupMember([FromBody] GroupMember addGroupMember)
        {
            if (addGroupMember == null)
                return BadRequest("addGroupMember object is null");

            // Xác định là Role "Member"
            addGroupMember.GroupRoleId = "680cebdfac700e1cb4c165b2";

            await _groupMemberService.AddGroupMember(addGroupMember);

            return CreatedAtAction(nameof(GetGroupMemberById), new { groupMemberId = addGroupMember.GroupMemberId }, addGroupMember);
        }

        [HttpDelete("delete/{groupMemberId}")]
        public async Task<IActionResult> DeleteGroup(string groupMemberId)
        {
            var group = await _groupMemberService.GetGroupMemberById(groupMemberId);
            if (group == null)
                return BadRequest("Group member not found");

            await _groupMemberService.DeleteGroupMember(groupMemberId);
            return Ok("Delete successfully!");
        }

        [HttpGet("users/in-group/{groupId}")]
        public async Task<IActionResult> GetUsersByGroupId(string groupId)
        {
            var users = await _groupMemberService.GetUsersInGroupAsync(groupId);
            if (users == null || users.Count == 0)
                return NotFound("No users found in this group.");

            return Ok(users);
        }

        [HttpGet("search-user-in-group/{groupId}")]
        public async Task<IActionResult> SearchUsersInGroup(string groupId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var users = await _groupMemberService.SearchUsersInGroupAsync(groupId, keyword);

            if (users.Count == 0)
                return NotFound("Not found members.");

            return Ok(users);
        }

        [HttpGet("list-request-to-join/{groupId}")]
        public async Task<IActionResult> GetJoinRequests(string groupId)
        {
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
        [HttpPost("request-to-join")]
        public async Task<IActionResult> RequestToJoinGroup([FromBody] RequestToJoinGroupRequestDTO requestToJoinGroup)
        {
            var result = await _groupMemberService.RequestToJoinGroupAsync(requestToJoinGroup.AccId, requestToJoinGroup.GroupId);
            if (result == null)
                return BadRequest(new { Success = false, Message = "You send already or you are member." });

            return Ok(new { Success = true, Message = "Send request to group successfuly", Data = result });
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


    }
}
