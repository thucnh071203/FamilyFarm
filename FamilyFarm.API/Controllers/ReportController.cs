using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
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
        [HttpPost("all")]
        public async Task<IActionResult> GetAllReport()
        {
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
        [HttpPost("all-pending")]
        public async Task<IActionResult> GetAllPending()
        {
            var reports = await _reportService.GetAll();
            var pendingReports = reports.Where(r => r.Status == "pending").ToList();
            return Ok(pendingReports);
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
        public async Task<IActionResult> Create([FromBody] Report report)
        {
            var existing = await _reportService.GetByPostAndReporter(report.PostId, report.ReporterId);
            if (existing != null)
                return Conflict("You have already reported this post");

            var result = await _reportService.Create(report);
            if (result == null)
                return BadRequest("Invalid PostId or AccId");

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
        [HttpPost("accept/{id}")]
        public async Task<IActionResult> Accept(string id)
        {
            var existing = await _reportService.GetById(id);
            if (existing == null)
                return NotFound("Report Not Found");

            existing.Status = "accepted";
            var result = await _reportService.Update(id, existing);
            if (result == null)
                return BadRequest("Invalid");

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
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(string id)
        {
            var existing = await _reportService.GetById(id);
            if (existing == null)
                return NotFound("Report Not Found");

            existing.Status = "rejected";
            var result = await _reportService.Update(id, existing);
            if (result == null)
                return BadRequest("Invalid");

            return Ok(result);
        }
    }
}
