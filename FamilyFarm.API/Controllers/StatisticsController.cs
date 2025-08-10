using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FamilyFarm.API.Controllers
{
    [Route("api/statistic")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        private readonly IAccountService _accountService;
        private readonly IAuthenticationService _authenService;

        public StatisticsController(IStatisticService statisticService, IAccountService accountService, IAuthenticationService authenService)
        {
            _statisticService = statisticService;
            _accountService = accountService;
            _authenService = authenService;
        }

        [HttpGet("totalPost")]
        public async Task<IActionResult> GetTotalPosts()
        {
            var count = await _statisticService.GetTotalPostCountAsync();
            return Ok(new { totalPosts = count });
        }

        [HttpGet("count-by-role")]
        public async Task<IActionResult> GetTotalAndGrowthByRole()
        {
            var roleIds = new List<string> { "68007b0387b41211f0af1d56", "68007b2a87b41211f0af1d57" };
            var result = await _accountService.GetTotalAndGrowthByRoleIdsAsync(roleIds);

            var response = result.ToDictionary(
                x => x.Key,
                x => new {
                    count = x.Value.Count,
                    growth = x.Value.Growth
                });

            return Ok(new
            {
                IsSuccess = true,
                Message = "Fetched role count and growth successfully",
                Data = response
            });
        }


        [HttpGet("growth")]
        public async Task<IActionResult> GetUserGrowthOverTime([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("fromDate must be earlier than or equal to toDate.");
            }

            try
            {
                var result = await _accountService.GetUserGrowthOverTimeAsync(fromDate, toDate);

                if (result == null)
                {
                    return NotFound("No data found.");
                }

                return Ok(result); // 200 OK with TotlaFarmerExpertDTO
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("user-growth")]
        public async Task<IActionResult> GetUserGrowth(DateTime? fromDate, DateTime? toDate)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return BadRequest();
            DateTime endDate = ((toDate ?? DateTime.Today).AddDays(1)).ToUniversalTime();
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
            if (top <= 0)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "The value of 'top' must be greater than 0.",
                    data = (object)null
                });
            }

            var result = await _statisticService.GetTopEngagedPostsAsync(top);
            return Ok(new
            {
                isSuccess = true,
                message = "Success",
                data = result
            });
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
            if (userStats == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "No user data found by province.",
                    data = (object)null
                });
            }

            return Ok(new
            {
                isSuccess = true,
                message = "User data by province retrieved successfully.",
                data = userStats
            });
        }


        //++++++++++++++++++

        [Authorize]
        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] string status)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            if (string.IsNullOrEmpty(accId))
                        return BadRequest("thiếu accId");

            var result = await _statisticService.GetBookingsByStatusAsync(accId, status);
            return Ok(result);
        }
       

        [HttpGet("time")]
        [Authorize]
        public async Task<IActionResult> GetBookingStatisticByTime(
         [FromQuery] int year,
         [FromQuery] string type)  // "month" hoặc "day"
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            Debug.WriteLine($"accId ........: {accId}");

            if (string.IsNullOrEmpty(accId) || year <= 0 || string.IsNullOrEmpty(type))
            {
                return BadRequest("Missing or invalid query parameters");
            }

            if (type.ToLower() == "month")
            {
                var result = await _statisticService.GetCountByMonthAsync(accId, year);
                return Ok(result);
            }
            else if (type.ToLower() == "day")
            {
                var result = await _statisticService.GetCountByDayAllMonthsAsync(accId, year);
                return Ok(result);
            }
            else
            {
                return BadRequest("Invalid type parameter. Use 'month' or 'day'");
            }
        }

        [Authorize]
        [HttpGet("popular-service-categories")]
        public async Task<IActionResult> GetPopularServiceCategories()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;



            if (string.IsNullOrEmpty(accId))
                return BadRequest("accId is required.");

            var result = await _statisticService.GetPopularServiceCategoriesAsync(accId);
            return Ok(result);
        }

 
        [Authorize]
        [HttpGet("most-booked-services")]
        public async Task<IActionResult> GetMostBookedServices()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            if (string.IsNullOrEmpty(accId))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Account ID (accId) is required.",
                    data = (object)null
                });
            }

            var result = await _statisticService.GetMostBookedServicesByExpertAsync(accId);

            return Ok(new
            {
                isSuccess = true,
                message = "Most booked services retrieved successfully.",
                data = result
            });
        }

        [Authorize]
        [HttpGet("expertRevenue")]
        public async Task<IActionResult> GetRevenueByExpert([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {

            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;



            if (string.IsNullOrEmpty(accId))
                return BadRequest("accId is required.");

            var result = await _statisticService.GetRevenueByExpertAsync(accId, from, to);
            return Ok(result);
        }


        [HttpGet("system")]
        public async Task<IActionResult> GetSystemRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var dto = await _statisticService.GetSystemRevenueAsync(from, to);
            return Ok(dto);
        }
    }
}
