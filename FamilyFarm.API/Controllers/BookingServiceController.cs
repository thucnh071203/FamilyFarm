using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
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
            var user = _authenService.GetDataFromToken();
            if (user == null)
                return Unauthorized("Missing or invalid token");

            if (user.RoleId != "68007b2a87b41211f0af1d57")
                return BadRequest("User is not expert");

            var result = await _bookingService.ExpertAcceptBookingService(bookingId);

            if (result == false)
                return BadRequest();

            //if (result == null)
            //    return BadRequest("Booking not found or invalid.");

            //if (result == false)
            //    return BadRequest("An error occurred during acceptance.");

            return Ok(result);
        }

        [HttpPut("reject-booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> RejectUpdateBookingStatus(string bookingId)
        {
            var result = await _bookingService.ExpertRejectBookingService(bookingId);

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
            if(accId == null) return Unauthorized();
            var result = await _bookingService.GetRequestBookingOfExpert(accId);

            if (result.Success == false)
                return BadRequest(result.Message);

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

            if (userClaims == null || string.IsNullOrEmpty(userClaims.AccId))
                return Unauthorized("Invalid token or missing user info");

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
            var user = _authenService.GetDataFromToken();
            if (user == null)
            {
                return Unauthorized();
            }
            if (serviceId == null || description == null)
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

        [HttpGet("expert/booking-paid")]
        [Authorize]
        public async Task<ActionResult> ListPaidBookingByExpert()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            var result = await _bookingService.GetListBookingPaid(accId);
            if (result == null)
                return BadRequest("Cannot get list paid booking of expert!");

            return Ok(result);
        }


        [HttpGet("expert/booking-unpaid")]
        [Authorize]
        public async Task<ActionResult> ListUnpaidBookingByExpert()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            var result = await _bookingService.GetListBookingUnpaid(accId);
            if (result == null)
                return BadRequest("Cannot get list unpaid booking of expert!");

            return Ok(result);
        }

        [HttpGet("get-by-id/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetBookingById(string bookingId)
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest("Id is required.");
            }

            var booking = await _bookingService.GetById(bookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            return Ok(booking);
        }

        [HttpGet("booking-completed")]
        [Authorize]
        public async Task<ActionResult> ListCompletedBooking()
        {
            var result = await _bookingService.GetListBookingCompleted();
            if (result == null)
                return BadRequest("Cannot get list unpaid booking of expert!");

            return Ok(result);
        }

        [HttpPut("request-extra")]
        [Authorize]
        public async Task<ActionResult> SendRequestExtraProcessByBooking([FromBody] CreateExtraProcessRequestDTO request)
        {
            if(request == null)
              return BadRequest();

            var result = await _bookingService.RequestExtraProcessByBooking(request);
            if (result == null)
                return BadRequest();

            if (result == false)
                return Conflict();

            return Ok();
        }

        [HttpGet("request-extra")]
        [Authorize]
        public async Task<ActionResult> GetListReuqestExtraProcess()
        {
            var userClaims = _authenService.GetDataFromToken();
            if(userClaims == null)
                return Unauthorized();

            var accId = userClaims.AccId;

            var result = await _bookingService.GetListExtraRequest(accId);

            if (result == null)
                return BadRequest();

            if(result.Success == false) 
                return Conflict();

            return Ok(result);
        }
    }
}
