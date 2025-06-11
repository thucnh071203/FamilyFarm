using System.Security.Cryptography;
using System.Text.RegularExpressions;
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

        [HttpGet("all-group-user")]//get all group of user
        [Authorize]
        public async Task<IActionResult> GetAllByUserid()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var groups = await _groupService.GetAllByUserId(accId);
            return Ok(groups);
        }

        [HttpGet("get-by-id/{groupId}")]
        public async Task<IActionResult> GetGroupById(string groupId)
        {
            var group = await _groupService.GetGroupById(groupId);
            return Ok(group);
        }

        [HttpGet("get-lastest")]
        [Authorize]
        public async Task<IActionResult> DeleteGroup()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _groupService.GetLatestGroupByCreator(account.AccId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromForm] GroupRequestDTO addGroup)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (string.IsNullOrWhiteSpace(addGroup.GroupName) || string.IsNullOrWhiteSpace(addGroup.PrivacyType))
                return BadRequest("GroupName and PrivacyType must not be empty.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (addGroup == null)
                return BadRequest("addGroup object is null");

            addGroup.AccountId = account.AccId;

            var result = await _groupService.CreateGroup(addGroup);
            return result.Success ? Ok(result) : BadRequest(result);
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

            if (updateGroup == null)
                return BadRequest("updateGroup object is null");

            updateGroup.AccountId = account.AccId;

            var result = await _groupService.UpdateGroup(groupId, updateGroup);
            return result.Success ? Ok(result) : BadRequest(result);
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

            var result = await _groupService.DeleteGroup(groupId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
