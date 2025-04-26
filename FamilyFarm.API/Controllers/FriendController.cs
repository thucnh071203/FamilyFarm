using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
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

        public FriendController(IFriendRequestService friendService)
        {
            _friendService = friendService;
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

    }
}
