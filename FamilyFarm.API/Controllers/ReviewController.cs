using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/review")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IAuthenticationService _authenService;

        public ReviewController(IReviewService reviewService, IAuthenticationService authenService)
        {
            _reviewService = reviewService;
            _authenService = authenService;
        }

        [HttpGet("get-by-service/{serviceId}")]
        public async Task<IActionResult> GetByServiceId(string serviceId)
        {
            var response = await _reviewService.GetByServiceIdAsync(serviceId);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpGet("summary/{serviceId}")]
        public async Task<IActionResult> GetSummary(string serviceId)
        {
            var response = await _reviewService.GetSummaryByServiceId(serviceId);
            return response.Success ? Ok(response) : NotFound(response);
        }


        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(string id)
        //{
        //    try
        //    {
        //        var review = await _reviewService.GetByIdAsync(id);
        //        return Ok(review);
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound(ex.Message);
        //    }
        //}

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ReviewRequestDTO request)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return BadRequest(new CommentResponseDTO { Success = false, Message = "Please Login!" });

            var response = await _reviewService.CreateAsync(request, account.AccId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        //[HttpPut("update/{id}")]
        //public async Task<IActionResult> Update(string id, [FromBody] ReviewRequestDTO request)
        //{
        //    var account = _authenService.GetDataFromToken();
        //    if (account == null)
        //        return BadRequest(new ReviewResponseDTO { Success = false, Message = "Please login!" });

        //    var response = await _reviewService.UpdateAsync(id, request, account.AccId);
        //    return response.Success ? Ok(response) : BadRequest(response);
        //}

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return BadRequest(new ReviewResponseDTO { Success = false, Message = "Please login!" });

            var response = await _reviewService.DeleteAsync(id, account.AccId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

    }
}
