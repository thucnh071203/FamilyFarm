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
        public async Task<IActionResult> CreateGroup([FromForm] GroupRequestDTO addGroup)
        {
            if (addGroup == null)
                return BadRequest("addGroup object is null");

            var addNewGroup = new FamilyFarm.Models.Models.Group
            {
                GroupId = null,
                OwnerId = addGroup.AccountId,
                GroupName = addGroup.GroupName,
                GroupAvatar = addGroup.GroupAvatar,
                GroupBackground = addGroup.GroupBackground,
                PrivacyType = addGroup.PrivacyType,
                CreatedAt = null,
                UpdatedAt = null,
                DeletedAt = null
            };

            await _groupService.CreateGroup(addNewGroup);

            return CreatedAtAction(nameof(GetGroupById), new { groupId = addNewGroup.GroupId }, addNewGroup);
        }

        [HttpPut("update/{groupId}")]
        public async Task<IActionResult> UpdateGroup(string groupId, [FromForm] GroupRequestDTO updateGroup)
        {
            var group = await _groupService.GetGroupById(groupId);
            if (group == null)
                return BadRequest("Group not found");

            if (group.OwnerId != updateGroup.AccountId)
            {
                return BadRequest("Account id does not match");
            }

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
