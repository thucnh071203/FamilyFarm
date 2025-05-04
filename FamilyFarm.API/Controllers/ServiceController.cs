using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServicingService _servicingService;

        public ServiceController(IServicingService servicingService)
        {
            _servicingService = servicingService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllServices()
        {
            var result = await _servicingService.GetAllService();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-by-provider/{providerId}")]
        public async Task<IActionResult> GetAllServicesByProvider(string providerId)
        {
            var result = await _servicingService.GetAllServiceByProvider(providerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("get-by-id/{serviceId}")]
        public async Task<IActionResult> GetServiceById(string serviceId)
        {
            var result = await _servicingService.GetServiceById(serviceId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateService([FromForm] ServiceRequestDTO service)
        {
            var result = await _servicingService.CreateService(service);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update/{serviceId}")]
        public async Task<IActionResult> UpdateService(string serviceId, [FromForm] ServiceRequestDTO service)
        {
            var result = await _servicingService.UpdateService(serviceId, service);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{serviceId}")]
        public async Task<IActionResult> DeleteService(string serviceId)
        {
            var result = await _servicingService.DeleteService(serviceId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
