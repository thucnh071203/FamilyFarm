using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/statistic")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        private readonly IAccountService _accountService;


        public StatisticsController(IStatisticService statisticService, IAccountService accountService)
        {
            _statisticService = statisticService;
            _accountService = accountService;
        }

      
        [HttpGet("count-by-role")]
        public async Task<IActionResult> CountByRole()
        {
            var roleIds = new List<string>
            {
                "68007b0387b41211f0af1d56", // Farmer
                "68007b2a87b41211f0af1d57"  // Expert
            };

            var result = await _accountService.GetTotalByRoleIdsAsync(roleIds);
            return Ok(result);
        }
   
        [HttpGet("growth")]
        public async Task<IActionResult> GetUserGrowthOverTime([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = await _accountService.GetUserGrowthOverTimeAsync(fromDate, toDate);
            return Ok(result);
        }

        //[HttpGet("user-growth")]
        //public async Task<IActionResult> GetUserGrowth(DateTime? fromDate, DateTime? toDate)
        //{
        //    DateTime endDate = toDate ?? DateTime.Today; // Nếu toDate null thì lấy ngày hôm nay
        //    DateTime startDate = fromDate ?? endDate.AddDays(-30); // Nếu fromDate null thì lấy 30 ngày trước đó

        //    if (fromDate > toDate)
        //    {
        //        return BadRequest(new
        //        {
        //            isSuccess = false,
        //            message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.",
        //            data = (object)null
        //        });
        //    }

        //    var data = await _accountService.GetUserGrowthOverTimeAsync(startDate, endDate);

        //    return Ok(new
        //    {
        //        isSuccess = true,
        //        message = $"User growth from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}.",
        //        data
        //    });
        //}

        [HttpGet("user-growth")]
        public async Task<IActionResult> GetUserGrowth(DateTime? fromDate, DateTime? toDate)
        {

            DateTime endDate = (toDate ?? DateTime.Today).AddDays(1);
            DateTime startDate = (fromDate ?? endDate.AddDays(-31)).ToUniversalTime();


            if (startDate > endDate)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.",
                    data = (object)null
                });
            }

            var data = await _accountService.GetUserGrowthOverTimeAsync(startDate, endDate);

            return Ok(new
            {
                isSuccess = data.IsSuccess,
                message = data.Message,
                data = data.Data
            });
        }


        [HttpGet("top-engaged")]
    
        public async Task<IActionResult> GetTopEngagedPosts([FromQuery] int top = 5)
        {
            var result = await _statisticService.GetTopEngagedPostsAsync(top);
            return Ok(result);
        }

       
        [HttpGet("weekly-growth")]
        public async Task<IActionResult> GetWeeklyBookingGrowth()
        {
            try
            {
                var result = await _statisticService.GetWeeklyBookingGrowthAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

    
        [HttpGet("most-active-members")]
        public async Task<IActionResult> GetMostActiveMembers(DateTime startDate, DateTime endDate)
        {
            if (startDate == null || endDate == null || startDate > endDate)
            {
                return BadRequest("Invalid date range.");
            }

            var mostActiveMembers = await _statisticService.GetMostActiveMembersAsync(startDate, endDate);
            return Ok(mostActiveMembers);
        }

        [HttpGet("users-by-province")]
  
        public async Task<ActionResult<List<UserByProvinceResponseDTO>>> GetUsersByProvince()
        {
            var userStats = await _statisticService.GetUsersByProvinceAsync();
            return Ok(userStats);
        }

    }
}
