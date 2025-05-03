using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IAuthenticationService _authenService;
        private readonly ISearchHistoryService _searchHistoryService;

        public PostController(IPostService postService, IAuthenticationService authenService, ISearchHistoryService searchHistoryService)
        {
            _postService = postService;
            _authenService = authenService;
            _searchHistoryService = searchHistoryService;
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
        [Authorize]
        public async Task<IActionResult> SearchPosts([FromQuery] string? keyword, [FromQuery] List<string>? categoryIds, [FromQuery] bool isAndLogic = false)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            // Call the service method to perform the search
            var posts = await _postService.SearchPosts(keyword, categoryIds, isAndLogic);
            if (keyword != null){
                var search = await _searchHistoryService.AddSearchHistory(accId, keyword);
            }
            if (!posts.Any())
                return NotFound("No post found!");

            // Return the posts wrapped in an OK response if search is successful
            return Ok(posts);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<CreatePostResponseDTO>> CreateNewPost([FromForm] CreatePostRequestDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            var result = await _postService.AddPost(username, request);

            if (result == null) 
                return BadRequest();

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("update")]
        //[HttpPut("update/{id}")]
        [Authorize]
        public async Task<ActionResult<UpdatePostResponseDTO>> UpdatePost([FromForm] UpdatePostRequestDTO request)
        {
            if (request == null)
                return BadRequest("Invalid data from request.");

            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            if (username == null) 
                return NotFound("Not found resource for this action.");

            var result = await _postService.UpdatePost(username, request);

            if(result == null) 
                return BadRequest();

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("hard-delete/{post_id}")]
        [Authorize]
        public async Task<ActionResult<DeletePostResponseDTO>> HardDeletedPost([FromRoute] string post_id)
        {
            if (post_id == null)
                return BadRequest("Invalid data from request.");

            var request = new DeletePostRequestDTO
            {
                PostId = post_id
            };

            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;

            var isDeletedSuccess = await _postService.DeletePost(acc_id, request);

            if (isDeletedSuccess == null)
                return BadRequest("Invalid data from request");

            if(isDeletedSuccess.Success == false)
                return NotFound(isDeletedSuccess);

            return Ok(isDeletedSuccess);
        }

        [HttpDelete("soft-delete/{post_id}")]
        [Authorize]
        public async Task<ActionResult<DeletePostResponseDTO>> SoftDeletedPost([FromRoute] string post_id)
        {
            if (post_id == null)
                return BadRequest("Invalid data from request.");

            var request = new DeletePostRequestDTO
            {
                PostId = post_id
            };

            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;

            var isDeletedSuccess = await _postService.TempDeleted(acc_id, request);

            if (isDeletedSuccess == null)
                return BadRequest("Invalid data from request");

            if (isDeletedSuccess.Success == false)
                return NotFound(isDeletedSuccess);

            return Ok(isDeletedSuccess);
        }

        [HttpPut("restore/{post_id}")]
        [Authorize]
        public async Task<ActionResult<DeletePostRequestDTO>> RestorePost([FromRoute] string post_id)
        {
            if (post_id == null)
                return BadRequest("Invalid data from request.");

            var request = new DeletePostRequestDTO
            {
                PostId = post_id
            };

            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;

            var isDeletedSuccess = await _postService.RestorePostDeleted(acc_id, request);

            if (isDeletedSuccess == null)
                return BadRequest("Invalid data from request");

            if (isDeletedSuccess.Success == false)
                return NotFound(isDeletedSuccess);

            return Ok(isDeletedSuccess);
        }

        [HttpGet("search-posts-in-group/{groupId}")]
        public async Task<IActionResult> SearchPostsInGroup(string groupId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var posts = await _postService.SearchPostsInGroupAsync(groupId, keyword);

            if (posts.Count == 0)
                return NotFound("No found posts.");

            return Ok(posts);
        }

        [HttpGet("search-posts-in-group-dto/{groupId}")]
        public async Task<IActionResult> SearchPostsInGroupDTO(string groupId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var response = await _postService.SearchPostsWithAccountAsync(groupId, keyword);

            return Ok(response);
        }



    }
}
