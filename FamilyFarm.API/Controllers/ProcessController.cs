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

        public ProcessController(IProcessService processService, IAuthenticationService authenService)
        {
            _processService = processService;
            _authenService = authenService;
        }

        [HttpGet("all-by-expert")]
        [Authorize]
        public async Task<IActionResult> GetAllProcessesByExpert()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetAllProcessByExpert(account.AccId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-by-farmer")]
        [Authorize]
        public async Task<IActionResult> GetAllProcessesByFarmer()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _processService.GetAllProcessByFarmer(account.AccId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-by-id/{processId}")]
        public async Task<IActionResult> GetProcessById(string processId)
        {
            var result = await _processService.GetProcessById(processId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateProcess([FromForm] ProcessRequestDTO process)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            process.ExpertId = account.AccId;

            var result = await _processService.CreateProcess(process);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("update/{processId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProcess(string processId, [FromForm] ProcessRequestDTO process)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            process.ExpertId = account.AccId;

            var result = await _processService.UpdateProcess(processId, process);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{processId}")]
        [Authorize]
        public async Task<IActionResult> DeleteService(string processId)
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
    }
}
