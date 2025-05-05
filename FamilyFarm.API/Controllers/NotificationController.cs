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
