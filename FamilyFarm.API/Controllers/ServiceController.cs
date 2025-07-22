using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServicingService _servicingService;
        private readonly IAuthenticationService _authenService;

        public ServiceController(IServicingService servicingService, IAuthenticationService authenService)
        {
            _servicingService = servicingService;
            _authenService = authenService;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllServices()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var result = await _servicingService.GetAllService();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-by-provider")]
        [Authorize]
        public async Task<IActionResult> GetAllServicesByProvider()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            var result = await _servicingService.GetAllServiceByProvider(account.AccId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-by-account/{accId}")]
        [Authorize]
        public async Task<IActionResult> GetAllServicesByAccId(string accId)
        {

            var result = await _servicingService.GetAllServiceByProvider(accId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-by-id/{serviceId}")]
        public async Task<IActionResult> GetServiceById(string serviceId)
        {
            var result = await _servicingService.GetServiceById(serviceId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateService([FromForm] ServiceRequestDTO service)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account.RoleId != "68007b2a87b41211f0af1d57")
            {
                return BadRequest(new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert",
                    Data = null
                });
            }

            service.ProviderId = account.AccId;

            var result = await _servicingService.CreateService(service);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> UpdateService(string serviceId, [FromForm] ServiceRequestDTO service)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account.RoleId != "68007b2a87b41211f0af1d57")
            {
                return BadRequest(new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert",
                    Data = null
                });
            }

            var result = await _servicingService.UpdateService(serviceId, service);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("change-status/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> ChangeStatusService(string serviceId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account.RoleId != "68007b2a87b41211f0af1d57")
            {
                return BadRequest(new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert",
                    Data = null
                });
            }

            var result = await _servicingService.ChangeStatusService(serviceId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> DeleteService(string serviceId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account.RoleId != "68007b2a87b41211f0af1d57")
            {
                return BadRequest(new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert",
                    Data = null
                });
            }

            var result = await _servicingService.DeleteService(serviceId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-detail-by-id/{serviceId}")]
        public async Task<IActionResult> GetDetailServiceById(string serviceId)
        {
            var result = await _servicingService.GetServiceDetail(serviceId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
