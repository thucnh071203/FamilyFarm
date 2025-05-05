using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IAuthenticationService _authenService;

        public NotificationController(INotificationService notificationService, IAuthenticationService authenService)
        {
            _notificationService = notificationService;
            _authenService = authenService;
        }

        /// <summary>
        /// Sends a notification to one or multiple receivers.
        /// </summary>
        /// <param name="request">The notification data to send, including content, receiver list, category, etc.</param>
        /// <returns>Returns a success or failure response.</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromForm] SendNotificationRequestDTO request)
        {
            var response = await _notificationService.SendNotificationAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        /// <summary>
        /// Retrieves all notifications for the currently logged-in user.
        /// </summary>
        /// <returns>A list of notifications and count of unread ones.</returns>
        [Authorize]
        [HttpGet("get-by-user")]
        public async Task<IActionResult> ListNotificationsForUser()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return NotFound("Please login");

            var notifications = await _notificationService.GetNotificationsForUserAsync(account.AccId);
            return Ok(notifications);
        }

        /// <summary>
        /// Marks a specific notification as read by its status ID.
        /// </summary>
        /// <param name="notifiStatusId">The notification status ID to mark as read.</param>
        /// <returns>Status result indicating success or failure.</returns>
        [Authorize]
        [HttpPut("mark-as-read/{notifiStatusId}")]
        public async Task<IActionResult> MarkAsReadByNotificationId(string notifiStatusId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return NotFound("Please login");

            var success = await _notificationService.MarkAsReadByNotificationIdAsync(notifiStatusId);
            if (!success)
            {
                return NotFound("No Notification found or user not authorized!");
            }

            return Ok("Notification marked as read");
        }

        /// <summary>
        /// Marks all notifications as read for the currently logged-in user.
        /// </summary>
        /// <returns>Status result indicating success or failure.</returns>
        [Authorize]
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsReadByUserId()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return NotFound("Please login");

            var success = await _notificationService.MarkAllAsReadByAccIdAsync(account.AccId);
            if (!success)
            {
                return NotFound("No Notification found or user not authorized!");
            }
            return Ok("All notifications marked as read.");
        }
    }
}
