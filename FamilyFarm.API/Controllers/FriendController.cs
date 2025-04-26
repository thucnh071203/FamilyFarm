using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
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
        /// Lấy danh sách lời mời kết bạn ĐÃ GỬI
        /// </summary>

        [HttpGet("requests/sent/{userId}")]
        public async Task<IActionResult> GetSentFriendRequests(string userId)
        {
            var sentRequests = await _friendService.GetAllSendFriendRequests(userId);
            return Ok(sentRequests);
        }

        /// <summary>
        /// Lấy danh sách lời mời kết bạn ĐÃ NHẬN
        /// </summary>
        [HttpGet("received-requests/{receiverId}")]
        public async Task<IActionResult> GetReceivedRequestsAsync(string receiverId)
        {
            // In giá trị nhận được từ request
            Debug.WriteLine($"Received receiverId: {receiverId}");

            // Gọi service để lấy dữ liệu
            var result = await _friendService.GetAllReceiveFriendRequests(receiverId);

            if (result.Count == 0)
            {
                return NotFound("No pending requests found.");
            }

            return Ok(result);
        }

    }
}
