using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/category-service")]
    [ApiController]
    public class CategoryServiceController : ControllerBase
    {
        private readonly ICategoryServicingService _categoryServicingService;

        public CategoryServiceController(ICategoryServicingService categoryServicingService)
        {
            _categoryServicingService = categoryServicingService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategoryServices()
        {
            var result = await _categoryServicingService.GetAllCategoryService();
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("get-by-id/{categoryServiceId}")]
        public async Task<IActionResult> GetCategoryServiceById(string categoryServiceId)
        {
            var result = await _categoryServicingService.GetCategoryServiceById(categoryServiceId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCategoryService([FromBody] CategoryService category)
        {
            var result = await _categoryServicingService.CreateCategoryService(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update/{categoryServiceId}")]
        public async Task<IActionResult> UpdateCategoryService(string categoryServiceId, [FromBody] CategoryService category)
        {
            var result = await _categoryServicingService.UpdateCategoryService(categoryServiceId, category);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpDelete("delete/{categoryServiceId}")]
        public async Task<IActionResult> DeleteCategoryService(string categoryServiceId)
        {
            var result = await _categoryServicingService.DeleteCategoryService(categoryServiceId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
