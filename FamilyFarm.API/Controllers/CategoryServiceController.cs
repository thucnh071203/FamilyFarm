using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/category-service")]
    [ApiController]
    public class CategoryServiceController : ControllerBase
    {
        private readonly ICategoryServicingService _categoryServicingService;
        private readonly IAuthenticationService _authenService;

        public CategoryServiceController(ICategoryServicingService categoryServicingService, IAuthenticationService authenticationService)
        {
            _categoryServicingService = categoryServicingService;
            _authenService = authenticationService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategoryServices()
        {
           
            var result = await _categoryServicingService.GetAllCategoryService();
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("all-for-admin")]
        [Authorize]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");
            var result = await _categoryServicingService.GetAllForAdmin();
            return result.Success ? Ok(result) : NotFound(result);
        }
        [HttpGet("get-by-id/{categoryServiceId}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryServiceById(string categoryServiceId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");
            var result = await _categoryServicingService.GetCategoryServiceById(categoryServiceId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateCategoryService([FromBody] CategoryService category)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");
            var result = await _categoryServicingService.CreateCategoryService(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update/{categoryServiceId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCategoryService(string categoryServiceId, [FromBody] CategoryService category)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");
            var result = await _categoryServicingService.UpdateCategoryService(categoryServiceId, category);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpDelete("delete/{categoryServiceId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategoryService(string categoryServiceId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");
            var result = await _categoryServicingService.DeleteCategoryService(categoryServiceId);
            return result.Success ? Ok(result) : NotFound(result);
        }
        [HttpPut("restore/{categoryServiceId}")]
        [Authorize]
        public async Task<IActionResult> Restore(string categoryServiceId)
        {
            var result = await _categoryServicingService.Restore(categoryServiceId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
