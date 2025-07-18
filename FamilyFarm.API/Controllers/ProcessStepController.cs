using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/process-step")]
    [ApiController]
    public class ProcessStepController : ControllerBase
    {
        private readonly IProcessService _processService;
        private readonly IAuthenticationService _authenService;

        public ProcessStepController(IProcessService processService, IAuthenticationService authenService)
        {
            _processService = processService;
            _authenService = authenService;
        }

        [HttpPost("result/create")]
        [Authorize]
        public async Task<IActionResult> CreateProcessStepResult([FromForm] ProcessStepResultRequestDTO request)
        {
            var result = await _processService.CreateProcessStepResult(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("result/{stepId}")]
        [Authorize]
        public async Task<IActionResult> GetProcessStepResults(string stepId)
        {
            var result = await _processService.GetProcessStepResultsByStepId(stepId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}