using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyFarm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryReactionController : ControllerBase
    {
        private readonly ICategoryReactionService _categoryReactionService;
        private readonly IAuthenticationService _authenService;

        public CategoryReactionController(ICategoryReactionService categoryReactionService, IAuthenticationService authenService)
        {
            _categoryReactionService = categoryReactionService;
            _authenService = authenService;
        }

        /// <summary>
        /// Retrieves all CategoryReactions that are not deleted.
        /// </summary>
        /// <returns>
        /// An IActionResult containing a list of all active CategoryReactions.
        /// Returns 200 OK with the list, even if empty.
        /// </returns>
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryReactionService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("all-available")]
        public async Task<IActionResult> GetAllAvailable()
        {
            var result = await _categoryReactionService.GetAllAvalableAsync();
            return Ok(result);
        }


        [HttpGet("getAllCategoryReaction")]
        public async Task<IActionResult> GetAllCategoryReaction()
        {
            var list = await _categoryReactionService.GetAllAsync();
            return Ok(new CategoryReactionResponse<List<CategoryReaction>>(true, "Lấy danh sách reaction thành công", list));
        }

        [HttpGet("getByIDCategoryReaction/{id}")]
        public async Task<IActionResult> GetByIdCategoryReaction(string id)
        {
            var item = await _categoryReactionService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "Không tìm thấy reaction", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Lấy thành công", item));
        }


        [HttpPost("createCategoryReaction")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CategoryReaction model)
        {
            var user = _authenService.GetDataFromToken();

            model.AccId = user.AccId;

            var existing = await _categoryReactionService.GetByIdAsync(model.CategoryReactionId);
            if (existing != null)
                return BadRequest(new CategoryReactionResponse<CategoryReaction>(false, "ID đã tồn tại", null));

            await _categoryReactionService.CreateAsync(model);
            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Tạo reaction thành công", model));
        }

        [HttpPut("updateCategoryReaction/{id}")]
        public async Task<IActionResult> UpdateCategoryReaction(string id, [FromBody] CategoryReaction model)
        {
            var user = _authenService.GetDataFromToken();
            model.AccId = user.AccId;
            model.IsDeleted = false;

            var result = await _categoryReactionService.UpdateAsync(id, model);
            if (!result)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "Không tìm thấy reaction để cập nhật", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Cập nhật thành công", model));
        }

        [HttpDelete("deleteCategoryReaction/{id}")]
        public async Task<IActionResult> DeleteCategoryReaction(string id)
        {
            var result = await _categoryReactionService.DeleteAsync(id);
            if (!result)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "Không tìm thấy reaction để xoá", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Xoá thành công"));
        }
    }
}