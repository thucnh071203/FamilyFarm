using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyFarm.Controllers
{
    [ApiController]
    [Route("api/category-reaction")]
    public class CategoryReactionController : ControllerBase
    {
        private readonly ICategoryReactionService _categoryReactionService;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthenticationService _authenService;

        public CategoryReactionController(ICategoryReactionService categoryReactionService, IAuthenticationService authenService, IUploadFileService uploadFileService)
        {
            _categoryReactionService = categoryReactionService;
            _authenService = authenService;
            _uploadFileService = uploadFileService;
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
            var list = await _categoryReactionService.GetAllAsync();
            return Ok(new CategoryReactionResponse<List<CategoryReaction>>(true, "Lấy danh sách reaction thành công", list));
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


        //[HttpGet("getAllCategoryReaction")]
        //public async Task<IActionResult> GetAllCategoryReaction()
        //{
        //    var list = await _categoryReactionService.GetAllAsync();
        //    return Ok(new CategoryReactionResponse<List<CategoryReaction>>(true, "Lấy danh sách reaction thành công", list));
        //}

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByIdCategoryReaction(string id)
        {
            var item = await _categoryReactionService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "Không tìm thấy reaction", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Lấy thành công", item));
        }


        //[HttpPost("create")]
        //[Authorize]
        //public async Task<IActionResult> Create([FromBody] CategoryReaction model)
        //{
        //    var user = _authenService.GetDataFromToken();

        //    model.AccId = user.AccId;

        //    var existing = await _categoryReactionService.GetByIdAsync(model.CategoryReactionId);
        //    if (existing != null)
        //        return BadRequest(new CategoryReactionResponse<CategoryReaction>(false, "ID đã tồn tại", null));

        //    await _categoryReactionService.CreateAsync(model);
        //    return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Tạo reaction thành công", model));
        //}
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CategoryReactionDTO request)
        {
            var user = _authenService.GetDataFromToken();

            // Tạo model để lưu vào DB
            var model = new CategoryReaction
            {
                CategoryReactionId = ObjectId.GenerateNewId().ToString(),
                ReactionName = request.ReactionName,
                AccId = user.AccId,
                IconUrl = "",
                IsDeleted = false,
            };

            // Upload file ảnh nếu có
            if (request.IconUrl != null)
            {
                var uploadResult = await _uploadFileService.UploadImage(request.IconUrl);
                model.IconUrl = uploadResult?.UrlFile ?? "";
            }

            await _categoryReactionService.CreateAsync(model);
            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Create reaction successfully!", model));
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCategoryReaction(string id, [FromForm] CategoryReactionDTO request)
        {
            var user = _authenService.GetDataFromToken();

            var exsiting = await _categoryReactionService.GetByIdAsync(id);

            exsiting.ReactionName = request.ReactionName;
            
            if (request.IconUrl != null)
            {
                var uploadResult = await _uploadFileService.UploadImage(request.IconUrl);
                exsiting.IconUrl = uploadResult?.UrlFile ?? "";
            }

            var result = await _categoryReactionService.UpdateAsync(id, exsiting);
            if (!result)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "No reaction found to update", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Update reaction successfully!", exsiting));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategoryReaction(string id)
        {
            var result = await _categoryReactionService.DeleteAsync(id);
            if (!result)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "No reaction found to delete", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Delete reaction successfully!"));
        }

        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreCategoryReaction(string id)
        {
            var result = await _categoryReactionService.RestoreAsync(id);
            if (!result)
                return NotFound(new CategoryReactionResponse<CategoryReaction>(false, "No reaction found to restore", null));

            return Ok(new CategoryReactionResponse<CategoryReaction>(true, "Restore reaction successfully!"));
        }
    }
}