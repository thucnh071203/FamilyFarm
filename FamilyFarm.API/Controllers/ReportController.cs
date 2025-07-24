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
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAuthenticationService _authenService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly IAuthenticationService _authenticationService;

        public ReportController(IReportService reportService, IAuthenticationService authenService, IPostService postService, INotificationService notificationService, IAuthenticationService authenticationService)
        {
            _reportService = reportService;
            _authenService = authenService;
            _postService = postService;
            _notificationService = notificationService;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Retrieves all reports.
        /// This endpoint fetches all reports from the report service.
        /// </summary>
        /// <returns>
        /// An IActionResult containing a list of all reports.
        /// If reports exist, it returns them with a 200 OK status.
        /// If no reports exist, it returns an empty list with a 200 OK status.
        /// </returns>
        [HttpGet("all")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllReport()
        {
            var user = _authenService.GetDataFromToken();
            if (user == null)
                return Unauthorized();

            //if (user.RoleName != "Admin")
            //    return Forbid();

            var reports = await _reportService.GetAll();
            return Ok(reports);
        }

        /// <summary>
        /// Retrieves all reports with a "pending" status.
        /// This endpoint filters reports to return only those that are in "pending" status.
        /// </summary>
        /// <returns>
        /// An IActionResult containing a list of all pending reports.
        /// If pending reports exist, it returns them with a 200 OK status.
        /// If no pending reports exist, it returns an empty list with a 200 OK status.
        /// </returns>
        [HttpGet("all-pending")]
        [Authorize]
        public async Task<IActionResult> GetAllPending()
        {
            var response = await _reportService.GetAll();

            if (response == null || response.Data == null)
            {
                return NotFound(new { message = "No reports found." });
            }

            var pendingReports = response.Data
                .Where(r => r.Report.Status == "pending")
                .ToList();

            return Ok(new
            {
                Success = true,
                Message = $"Found {pendingReports.Count} pending reports.",
                Data = pendingReports
            });
        }

        [HttpGet("get-by-id/{id}")]
        [Authorize]
        public async Task<IActionResult> GeById(string id)
        {
            var report = await _reportService.GetById(id);
            return Ok(report);
        }

        /// <summary>
        /// Creates a new report for a given post by a reporter.
        /// Checks if the reporter has already reported the same post.
        /// If the report already exists, returns a conflict response.
        /// If the report is successfully created, returns the result.
        /// If there is an error with the PostId or ReporterId, returns a bad request.
        /// </summary>
        /// <param name="report">The report object containing the report details to be created.</param>
        /// <returns>
        /// - Conflict if the reporter has already reported the same post
        /// - BadRequest if the PostId or ReporterId are invalid
        /// - Ok with the created report if the operation is successful
        /// </returns>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReportRequestDTO request)
        {
            // Lấy thông tin tài khoản từ token
            var account = _authenService.GetDataFromToken();
            if (account == null)
            {
                return Unauthorized(new ReportResponseDTO
                {
                    Success = false,
                    Message = "User is not authenticated.",
                    Data = null
                });
            }

            // Tạo DTO với ReporterId từ token
            var reportRequest = new CreateReportRequestDTO
            {
                PostId = request.PostId,
                Reason = request.Reason
            };

            // Kiểm tra xem báo cáo đã tồn tại chưa
            var existing = await _reportService.GetByPostAndReporter(request.PostId, account.AccId);
            if (existing != null)
            {
                return Conflict(new ReportResponseDTO
                {
                    Success = false,
                    Message = "You have already reported this post.",
                    Data = null
                });
            }

            // Gọi service để tạo báo cáo
            var result = await _reportService.CreateAsync(reportRequest, account.AccId);
            if (!result.Success || result.Data == null)
            {
                return BadRequest(new ReportResponseDTO
                {
                    Success = false,
                    Message = result.Message ?? "Invalid PostId.",
                    Data = null
                });
            }

            // Trả về kết quả thành công
            return Ok(result);
        }

        /// <summary>
        /// Accepts a report and updates its status to "accepted".
        /// This endpoint is used to change the status of a report to "accepted".
        /// </summary>
        /// <param name="id">The unique identifier of the report to be accepted.</param>
        /// <returns>
        /// An IActionResult:
        /// - If the report exists, updates the report status to "accepted" and returns the updated report with a 200 OK status.
        /// - If the report does not exist, returns a 404 Not Found with a message "Report Not Found".
        /// - If the update fails, returns a 400 Bad Request with a message "Invalid".
        /// </returns>
        [HttpPut("accept/{id}")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Accept(string id)
        {
            var account = _authenticationService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            //if (account.RoleName?.ToUpper() != "ADMIN")
            //    return Forbid();

            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid report ID.");

            var existing = await _reportService.GetById(id);
            if (existing == null || existing.Data == null)
                return NotFound("Cannot found the report!");

            existing.Data.Report.Status = "accepted";
            existing.Data.Report.HandledById = account.AccId;
            var result = await _reportService.Update(id, existing.Data.Report);
            if (result == null)
                return BadRequest("Update report failed.");

            var postRequest = new DeletePostRequestDTO
            {
                PostId = existing.Data.Report.PostId
            };

            SendNotificationRequestDTO notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { existing.Data.Post.Post.AccId },
                SenderId = account.AccId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3b",
                TargetId = null,
                TargetType = null,
                Content = "Your post was deleted because it was reported with reason \"" + existing.Data.Report.Reason + "\"."
            };

            await _notificationService.SendNotificationAsync(notiRequest);
            await _postService.DeletePost(postRequest);

            return Ok(result);
        }


        /// <summary>
        /// Rejects a report and updates its status to "rejected".
        /// This endpoint is used to change the status of a report to "rejected".
        /// </summary>
        /// <param name="id">The unique identifier of the report to be rejected.</param>
        /// <returns>
        /// An IActionResult:
        /// - If the report exists, updates the report status to "rejected" and returns the updated report with a 200 OK status.
        /// - If the report does not exist, returns a 404 Not Found with a message "Report Not Found".
        /// - If the update fails, returns a 400 Bad Request with a message "Invalid".
        /// </returns>
        [HttpPut("reject/{id}")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Reject(string id)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
            {
                return Unauthorized("Unauthorized");
            }

            var existing = await _reportService.GetById(id);
            if (existing == null)
                return NotFound("Report Not Found");

            existing.Data.Report.Status = "rejected";
            var result = await _reportService.Update(id, existing.Data.Report);
            if (result == null)
                return BadRequest("Invalid");

            return Ok(result);
        }
    }
}
