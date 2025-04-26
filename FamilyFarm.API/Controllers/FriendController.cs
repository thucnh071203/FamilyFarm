using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FamilyFarm.API.Controllers
{
    [Route("api/friend")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRequestService _friendService;
        private readonly IFriendService _serviceOfFriend;
        private readonly IAuthenticationService _authenService;

        public FriendController(IFriendRequestService friendService,IFriendService service, IAuthenticationService authorizationService)
        {
            _friendService = friendService;
            _serviceOfFriend = service;
            _authenService = authorizationService;
        }
        /// <summary>
        /// Retrieves the list of friend requests SENT by a user that are still in PENDING status.
        /// </summary>
        /// <param name="userId">The ID of the user who sent the friend requests.</param>
        /// <returns>A list of pending sent friend requests, or an empty list if none found.</returns>

        [HttpGet("requests-sent/{userId}")]
        public async Task<IActionResult> GetSendRequest(string userId)
        {
            var pendingReports = await _friendService.GetAllSendFriendRequests(userId);

            if (pendingReports == null || !pendingReports.Any())
            {
                return Ok(new List<Friend>());
            }

            return Ok(pendingReports);
        }

        /// <summary>
        /// Retrieves the list of friend requests RECEIVED by a user that are still in PENDING status.
        /// </summary>
        /// <param name="userId">The ID of the user who received the friend requests.</param>
        /// <returns>A list of pending received friend requests, or an empty list if none found.</returns>

        [HttpGet("requests-receive/{userId}")]
        public async Task<IActionResult> GetReceiveRequest(string userId)
        {
            var pendingReports = await _friendService.GetAllReceiveFriendRequests(userId);

            if (pendingReports == null || !pendingReports.Any())
            {
                return Ok(new List<Friend>());
            }

            return Ok(pendingReports);
        }
       
        [HttpGet("list-friend")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFriends()
        {
            var username = _authenService.GetDataFromToken();

            var result = await _serviceOfFriend.GetListFriends(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-friend-other")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFriends(string username)
        {
            var result = await _serviceOfFriend.GetListFriends(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-follower")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFollower()
        {
            var username = _authenService.GetDataFromToken();

            var result = await _serviceOfFriend.GetListFollower(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-following")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFollowing()
        {
            var username = _authenService.GetDataFromToken();

            var result = await _serviceOfFriend.GetListFollowing(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("unfriend")]
        [Authorize]
        public async Task<ActionResult> Unfriend(string receiver)
        {
            var username = _authenService.GetDataFromToken();

            var result = await _serviceOfFriend.Unfriend(username, receiver);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }

    }
}
