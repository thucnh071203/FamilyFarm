using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/group-role")]
    [ApiController]
    public class GroupRoleController : ControllerBase
    {
        private readonly IGroupRoleService _groupRoleService;

        public GroupRoleController(IGroupRoleService groupRoleService)
        {
            _groupRoleService = groupRoleService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroupRole()
        {
            var groupRoles = await _groupRoleService.GetAllGroupRole();
            return Ok(groupRoles);
        }

        [HttpGet("get-by-id/{groupRoleId}")]
        public async Task<IActionResult> GetGroupRoleById(string groupRoleId)
        {
            var groupRole = await _groupRoleService.GetGroupRoleById(groupRoleId);
            return Ok(groupRole);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroupRole([FromBody] GroupRole addGroupRole)
        {
            if (addGroupRole == null)
                return BadRequest("addGroupRole object is null");

            await _groupRoleService.CreateGroupRole(addGroupRole);

            return CreatedAtAction(nameof(GetGroupRoleById), new { groupRoleId = addGroupRole.GroupRoleId }, addGroupRole);
        }

        [HttpPut("update/{groupRoleId}")]
        public async Task<IActionResult> UpdateGroupRole(string groupRoleId, [FromBody] GroupRole updateGroupRole)
        {
            var groupRole = await _groupRoleService.GetGroupRoleById(groupRoleId);
            if (groupRole == null)
                return BadRequest("Group role not found");

            groupRole.GroupRoleName = updateGroupRole.GroupRoleName;
            groupRole.GroupRoleDescripton = updateGroupRole.GroupRoleDescripton;

            await _groupRoleService.UpdateGroupRole(groupRoleId, updateGroupRole);

            return Ok(new
            {
                message = "Group role updated successfully",
                data = updateGroupRole
            });
        }

        [HttpDelete("delete/{groupRoleId}")]
        public async Task<IActionResult> DeleteGroupRole(string groupRoleId)
        {
            var groupRole = await _groupRoleService.GetGroupRoleById(groupRoleId);
            if (groupRole == null)
                return BadRequest("Group role not found");

            await _groupRoleService.DeleteGroupRole(groupRoleId);
            return Ok("Delete successfully!");
        }

    }
}
