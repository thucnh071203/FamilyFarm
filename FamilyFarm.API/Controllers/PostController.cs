using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="keyword"></param>
        ///// <returns></returns>
        //[HttpGet("search/{keyword}")]
        //public async Task<IActionResult> SearchPostsByKeyword(string keyword)
        //{

        //    var posts = await _postService.SearchPostsByKeyword(keyword);
        //    return Ok(posts);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="categoryIds"></param>
        ///// <param name="isAndLogic"></param>
        ///// <returns></returns>
        //[HttpGet("search/categories")]
        //public async Task<IActionResult> SearchPostsByCategories([FromQuery] List<string> categoryIds, [FromQuery] bool isAndLogic = false)
        //{
        //    var posts = await _postService.SearchPostsByCategories(categoryIds, isAndLogic);
        //    return Ok(posts);
        //}

        /// <summary>
        /// Handles the HTTP GET request to search for posts based on a keyword and/or category IDs.
        /// It delegates the actual search logic to the service layer and returns the search results as a response.
        /// </summary>
        /// <param name="keyword">The keyword to search for in the post content. Can be null or empty.</param>
        /// <param name="categoryIds">A list of category IDs to filter the posts by. Can be null or empty.</param>
        /// <param name="isAndLogic">A boolean value indicating whether to use AND logic (true) or OR logic (false) 
        /// for filtering posts based on category membership. Defaults to false (OR logic).</param>
        /// <returns>A response containing the list of posts that match the search criteria, or an error message in case of failure.</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] string? keyword, [FromQuery] List<string>? categoryIds, [FromQuery] bool isAndLogic = false)
        {
            // Call the service method to perform the search
            var posts = await _postService.SearchPosts(keyword, categoryIds, isAndLogic);

            // Return the posts wrapped in an OK response if search is successful
            return Ok(posts);
        }
    }
}
