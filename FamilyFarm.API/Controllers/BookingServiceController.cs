using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FamilyFarm.API.Controllers
{
    [Route("api/booking-service")]
    [ApiController]
    public class BookingServiceController : ControllerBase
    {
        private readonly IBookingServiceService _bookingService;
        private readonly IAuthenticationService _authenService;
        public BookingServiceController(IBookingServiceService bookingService, IAuthenticationService authenService)
        {
            _bookingService = bookingService;
            _authenService = authenService;
        }

        //[HttpPost("request-booking/{serviceId}")]
        //[Authorize]
        //public async Task<ActionResult> SendRequestBookingService(string serviceId)
        //{
        //    var userClaims = _authenService.GetDataFromToken();
        //    var username = userClaims?.Username;

        //    var result = await _bookingService.SendRequestBooking(username, serviceId);

        //    if (result == false)
        //        return BadRequest();

        //    return Ok(result);
        //}

        //[HttpPut("cancel-booking/{bookingId}")]
        //[Authorize]
        //public async Task<ActionResult> CancelBookingService(string bookingId)
        //{
        //    var result = await _bookingService.CancelBookingService(bookingId);

        //    if (result == false)
        //        return BadRequest();

        //    return Ok(result);
        //}

        //[HttpPut("reject-booking/{bookingId}")]
        //[Authorize]
        //public async Task<ActionResult> RejectBookingService(string bookingId)
        //{
        //    var result = await _bookingService.ExpertRejectBookingService(bookingId);

        //    if (result == false)
        //        return BadRequest();

        //    return Ok(result);
        //}

        [HttpPut("accept-booking/{bookingId}")]
        [Authorize]
        public async Task<ActionResult> ExpertAcceptBooking(string bookingId)
        {
            var result = await _bookingService.ExpertAcceptBookingService(bookingId);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("expert-list-request-booking")]
        [Authorize]
        public async Task<ActionResult> ListRequestBookingOfExpert()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _bookingService.GetRequestBookingOfExpert(accId);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok(result);
        }
        [HttpPut("expert-response-booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> UpdateBookingStatus(string bookingId, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(bookingId)) return BadRequest();
            bool? result = null;

            switch (status.ToLower())
            {
                case "accept":
                    result = await _bookingService.ExpertAcceptBookingService(bookingId);
                    break;
                case "reject":
                    result = await _bookingService.ExpertRejectBookingService(bookingId);
                    break;
                default:
                    return BadRequest("Invalid status.");
            }

            if (result !=true)// false hoặc null đều là lỗi
                return BadRequest();

            return Ok(result);
        }


        [HttpGet("expert-all-booking")]
        [Authorize]
        public async Task<ActionResult> ListBookingOfExpert()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _bookingService.GetAllBookingOfExpert(accId);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("farmer-list-request-booking")]
        [Authorize]
        public async Task<ActionResult> ListRequestBookingOfFarmer()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _bookingService.GetRequestBookingOfFarmer(accId);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok(result);
        }
        [HttpGet("farmer-all-booking")]
        [Authorize]
        public async Task<ActionResult> ListBookingOfFarmer()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _bookingService.GetAllBookingOfFarmer(accId);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("request/{serviceId}")]
        [Authorize]
        public async Task<ActionResult> CreateBookingService([FromBody] string? description, [FromRoute] string? serviceId)
        {
            if(serviceId == null || description == null)
            {
                return BadRequest("Data unvalid!");
            }

            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            var result = await _bookingService.RequestToBookingService(accId, serviceId, description);

            if (result == null || result == false)
                return BadRequest("Cannot booking!");

            return Ok("Booking service successfully!");
        }

        [HttpPut("cancel-booking/{bookingId}")]
        [Authorize]
        public async Task<ActionResult> CancelBookingService([FromRoute] string? bookingId)
        {
            if (string.IsNullOrEmpty(bookingId))
                return BadRequest("Data invalid");

            var result = await _bookingService.CancelBookingService(bookingId);
            if (result == null || result == false)
                return BadRequest("Cannot cancel");

            return Ok(result);
        }
    }
}
