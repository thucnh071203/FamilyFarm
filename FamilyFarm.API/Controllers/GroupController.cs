using System.Text.RegularExpressions;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroup()
        {
            var groups = await _groupService.GetAllGroup();
            return Ok(groups);
        }

        [HttpGet("get-by-id/{groupId}")]
        public async Task<IActionResult> GetGroupById(string groupId)
        {
            var group = await _groupService.GetGroupById(groupId);
            return Ok(group);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] FamilyFarm.Models.Models.Group addGroup)
        {
            if (addGroup == null)
                return BadRequest("addGroup object is null");

            await _groupService.CreateGroup(addGroup);

            return CreatedAtAction(nameof(GetGroupById), new { id = addGroup.GroupId }, addGroup);
        }

        [HttpPut("update/{groupId}")]
        public async Task<IActionResult> UpdateGroup(string groupId, [FromBody] GroupRequestDTO updateGroup)
        {
            var group = await _groupService.GetGroupById(groupId);
            if (group == null)
                return BadRequest("Group not found");

            group.GroupName = updateGroup.GroupName;
            group.GroupAvatar = updateGroup.GroupAvatar;
            group.GroupBackground = updateGroup.GroupBackground;
            group.PrivacyType = updateGroup.PrivacyType;

            await _groupService.UpdateGroup(groupId, group);

            return Ok(new
            {
                message = "Group updated successfully",
                data = updateGroup
            });
        }

        [HttpDelete("delete/{groupId}")]
        public async Task<IActionResult> DeleteGroup(string groupId)
        {
            var group = await _groupService.GetGroupById(groupId);
            if (group == null)
                return BadRequest("Group not found");

            await _groupService.DeleteGroup(groupId);
            return Ok("Delete successfully!");
        }
    }
}
