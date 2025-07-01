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

    }
}
