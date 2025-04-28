using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/role-in-group")]
    [ApiController]
    public class RoleInGroupController : ControllerBase
    {
        private readonly IRoleInGroupService _roleInGroupService;

        public RoleInGroupController(IRoleInGroupService roleInGroupService)
        {
            _roleInGroupService = roleInGroupService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoleInGroup()
        {
            var roleInGroups = await _roleInGroupService.GetAllRoleInGroup();
            return Ok(roleInGroups);
        }

        [HttpGet("get-by-id/{groupRoleId}")]
        public async Task<IActionResult> GetRoleInGroupById(string groupRoleId)
        {
            var roleInGroups = await _roleInGroupService.GetRoleInGroupById(groupRoleId);
            return Ok(roleInGroups);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoleInGroup([FromBody] RoleInGroup addRoleInGroup)
        {
            if (addRoleInGroup == null)
                return BadRequest("addRoleInGroup object is null");

            await _roleInGroupService.CreateRoleInGroup(addRoleInGroup);

            return CreatedAtAction(nameof(GetRoleInGroupById), new { groupRoleId = addRoleInGroup.GroupRoleId }, addRoleInGroup);
        }

        [HttpPut("update/{groupRoleId}")]
        public async Task<IActionResult> UpdateRoleInGroup(string groupRoleId, [FromBody] RoleInGroup updateRoleInGroup)
        {
            var roleInGroup = await _roleInGroupService.GetRoleInGroupById(groupRoleId);
            if (roleInGroup == null)
                return BadRequest("Group role not found");

            await _roleInGroupService.UpdateRoleInGroup(groupRoleId, updateRoleInGroup);

            return Ok(new
            {
                message = "Group role updated successfully",
                data = updateRoleInGroup
            });
        }

        [HttpDelete("delete/{groupRoleId}")]
        public async Task<IActionResult> DeleteRoleInGroup(string groupRoleId)
        {
            var roleInGroup = await _roleInGroupService.GetRoleInGroupById(groupRoleId);
            if (roleInGroup == null)
                return BadRequest("Group role not found");

            await _roleInGroupService.DeleteRoleInGroup(groupRoleId);
            return Ok("Delete successfully!");
        }
    }
}
