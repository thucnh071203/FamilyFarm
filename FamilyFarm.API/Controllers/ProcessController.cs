using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/process")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        private readonly IProcessService _processService;
        private readonly IAuthenticationService _authenService;
        private readonly IUploadFileService _uploadFileService;

        public ProcessController(IProcessService processService, IAuthenticationService authenService, IUploadFileService uploadFileService)
        {
            _processService = processService;
            _authenService = authenService;
            _uploadFileService = uploadFileService;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllProcesses()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetAllProcess();

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-by-id/{serviceId}")]
        public async Task<IActionResult> GetProcessById(string serviceId)
        {
            var result = await _processService.GetProcessById(serviceId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("get-by-process-id/{processId}")]
        [Authorize]
        public async Task<IActionResult> GetProcessByProcessId(string processId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Not permission");

            var result = await _processService.GetProcessByProcessId(processId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateProcess([FromBody] ProcessRequestDTO process)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            //process.ExpertId = account.AccId;

            var result = await _processService.CreateProcess(process);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update/{processId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProcess(string processId, [FromBody] ProcessUpdateRequestDTO process)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            //process.ExpertId = account.AccId;

            var result = await _processService.UpdateProcess(processId, process);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{processId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProcess(string processId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account.RoleId != "68007b2a87b41211f0af1d57")
            {
                return BadRequest(new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert",
                    Data = null
                });
            }

            var result = await _processService.DeleteProcess(processId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchProcessByKeyword([FromQuery] string? keyword)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetAllProcessByKeyword(keyword);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        //[HttpGet("filter")]
        //[Authorize]
        //public async Task<IActionResult> FilterProcessByStatus([FromQuery] string? status)
        //{
        //    var account = _authenService.GetDataFromToken();
        //    if (account == null)
        //        return Unauthorized("Invalid token or user not found.");

        //    if (!ObjectId.TryParse(account.AccId, out _))
        //        return BadRequest("Invalid AccIds.");

        //    var result = await _processService.FilterProcessByStatus(status, account.AccId);
        //    return result.Success ? Ok(result) : BadRequest(result);
        //}

        [HttpPost("upload-images")]
        [Authorize]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var result = await _uploadFileService.UploadListImage(files);
            return Ok(result);
        }

        [HttpPost("subprocess")]
        [Authorize]
        public async Task<IActionResult> CreateSubprocess([FromBody] CreateSubprocessRequestDTO request)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.CreateSubprocess(account.AccId, request);
            if (result == null)
                return BadRequest("Data is invalid. Cannot create subprocess!");

            return result == true ? Ok(new { success = result }) : BadRequest("Create subprocess fail!");
        }

        [HttpGet("subprocesses/expert-self-view")]
        [Authorize]
        public async Task<IActionResult> GetListSubprocessByExpert()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetListSubprocessByExpert(account.AccId);
            if (result == null)
                return BadRequest("Error while retrieving list");

            if (result.Success == false)
                return NotFound(result.Message);

            return Ok(result);
        }

        [HttpGet("subprocesses/farmer-self-view")]
        [Authorize]
        public async Task<IActionResult> GetListSubprocessByFarmer()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetListSubprocessByFarmer(account.AccId);
            if (result == null)
                return BadRequest("Error while retrieving list");

            if (result.Success == false)
                return NotFound(result.Message);

            return Ok(result);
        }

        [HttpGet("sub-process-completed")]
        [Authorize]
        public async Task<ActionResult> ListCompletedSubProcess()
        {
            var result = await _processService.GetListSubProcessCompleted();
            if (result == null)
                return BadRequest("Cannot get list completed sub process!");

            return Ok(result);
        }

        [HttpGet("subprocess/check-completed/{subprocessId}")]
        [Authorize]
        public async Task<ActionResult> CheckCompletedSubprocess([FromRoute] string? subprocessId)
        {
            if (string.IsNullOrEmpty(subprocessId))
                return BadRequest("Request is invalid.");

            var result = await _processService.IsCompletedSubprocess(subprocessId);

            if (result == null)
                return BadRequest("Cannot checked completed.");

            if(result == false) 
                return NotFound("Uncompleted");

            return Ok("Subprocess is completed.");
        }

        [HttpPut("confirm-subprocess")]
        [Authorize]
        public async Task<ActionResult> ConfirmSubprocess([FromBody] ConfirmSubprocessRequestDTO request)
        {
            if (request == null)
                return BadRequest("Data invalid.");

            var user = _authenService.GetDataFromToken();
            if(user == null)
                return Unauthorized();

            var result = await _processService.ConfirmSubprocess(request.SubprocessId, request.BookingServiceid);

            if (result == null)
                return BadRequest("Server cannot process request.");

            if (result == false)
                return Conflict("Confirm fail.");

            return Ok("Confirm successfully");
        }
    }
}
