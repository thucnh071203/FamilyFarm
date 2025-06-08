using System.Text.RegularExpressions;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IAuthenticationService _authenService;

        public GroupController(IGroupService groupService, IAuthenticationService authenService)
        {
            _groupService = groupService;
            _authenService = authenService;
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
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromForm] GroupRequestDTO addGroup)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (addGroup == null)
                return BadRequest("addGroup object is null");

            var addNewGroup = new FamilyFarm.Models.Models.Group
            {
                GroupId = null,
                OwnerId = account.AccId,
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
        [Authorize]
        public async Task<IActionResult> UpdateGroup(string groupId, [FromForm] GroupRequestDTO updateGroup)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var group = await _groupService.GetGroupById(groupId);
            if (group == null)
                return BadRequest("Group not found");

            if (group.OwnerId != account.AccId)
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
        [Authorize]
        public async Task<IActionResult> DeleteGroup(string groupId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var group = await _groupService.GetGroupById(groupId);
            if (group == null)
                return BadRequest("Group not found");

            await _groupService.DeleteGroup(groupId);

            return Ok("Delete successfully!");
        }
    }
}
