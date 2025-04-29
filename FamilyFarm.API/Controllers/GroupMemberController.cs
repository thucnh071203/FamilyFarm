using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
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
    }
}
