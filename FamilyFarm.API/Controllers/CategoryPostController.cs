using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/category-post")]
    [ApiController]
    public class CategoryPostController : ControllerBase
    {
        private readonly ICategoryPostService _categoryPostService;
        private readonly IAuthenticationService _authenticationService;
        public CategoryPostController(ICategoryPostService categoryPostService, IAuthenticationService authenticationService)
        {
            _categoryPostService = categoryPostService;
            _authenticationService = authenticationService;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<CategoryPostResponseDTO?>> GetListCategory()
        {
            var userClaims = _authenticationService.GetDataFromToken();
            if (userClaims == null) return Unauthorized();
            var list = await _categoryPostService.GetListCategory();
            if (list.Success != true) return BadRequest(list.MessageError);
            return Ok(list);
        }
        [HttpGet("get-by-id/{catPostId}")]
        [Authorize]
        public async Task<ActionResult<Category?>> GetCategoryOfPostById(string catPostId)
        {
            var userClaims = _authenticationService.GetDataFromToken();
            if (userClaims == null) return Unauthorized();
            var cat = await _categoryPostService.GetCategoryById(catPostId);
            if (cat == null) return BadRequest();
            return Ok(cat);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<CategoryPostResponseDTO?>> CreateCategoryOfPost([FromBody] Category category)
        {
            var userClaims = _authenticationService.GetDataFromToken();
            if (userClaims == null) return Unauthorized();
            var create = category;
            create.AccId = userClaims.AccId;
            var cat = await _categoryPostService.Create(create);
            if (cat.Success != true) return BadRequest(cat.MessageError);
            return Ok(cat);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<CategoryPostResponseDTO?>> EditCategoryOfPost([FromBody] Category category)
        {
            var userClaims = _authenticationService.GetDataFromToken();
            if (userClaims == null) return Unauthorized();
            var cat = await _categoryPostService.Update(category);
            if (cat.Success != true) return BadRequest(cat.MessageError);
            return Ok(cat);
        }

        [HttpDelete("delete/{catId}")]
        [Authorize]
        public async Task<ActionResult<CategoryPostResponseDTO?>> DeleteCategoryOfPost(string catId)
        {
            var userClaims = _authenticationService.GetDataFromToken();
            if (userClaims == null) return Unauthorized();
            var cat = await _categoryPostService.Delete(catId);
            if (cat.Success != true) return BadRequest(cat.MessageError);
            return Ok(cat);
        }
    }
}
