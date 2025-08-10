using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly ISavedPostService _savedPostService;
        private readonly ICohereService _cohereService;

        public PostController(IPostService postService, IAuthenticationService authenService, ISearchHistoryService searchHistoryService, ISavedPostService savedPostService, ICohereService cohereService)
        {
            _postService = postService;
            _authenService = authenService;
            _searchHistoryService = searchHistoryService;
            _savedPostService = savedPostService;
            _cohereService = cohereService;
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
        public async Task<IActionResult> SearchPosts([FromQuery] string? keyword, [FromQuery] List<string> categoryIds, [FromQuery] bool isAndLogic = false)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            // Call the service method to perform the search
            var response = await _postService.SearchPosts(keyword, categoryIds, isAndLogic);

            if (keyword != null)
            {
                await _searchHistoryService.AddSearchHistory(accId, keyword);
            }

            if (response == null || response.Success == false)
                return BadRequest(response);

            // Return the posts wrapped in an OK response if search is successful
            return Ok(response);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> CreateNewPost([FromForm] CreatePostRequestDTO request)
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
        public async Task<ActionResult<PostResponseDTO>> UpdatePost([FromForm] UpdatePostRequestDTO request)
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

        /// <summary>
        /// Retrieves a post by its unique identifier.
        /// </summary>
        /// <param name="post_id">The unique ID of the post to retrieve.</param>
        /// <returns>
        /// Returns an <see cref="ActionResult{T}"/> containing the post data if found;
        /// otherwise, returns a 400 BadRequest if the ID is invalid or a 404 NotFound if the post does not exist.
        /// </returns>
        [HttpGet("get-by-id/{post_id}")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> GetPostById(string post_id)
        {
            if (post_id == null)
                return BadRequest("Invalid data from request.");

            var post = await _postService.GetPostById(post_id);

            if (post?.Success == false)
                return NotFound(post);

            return Ok(post);
        }

        /// <summary>
        /// Retrieves all posts associated with the currently authenticated account.
        /// </summary>
        /// <returns>
        /// Returns an containing the list of posts for the current user;
        /// otherwise, returns a 400 BadRequest if the token is invalid or a 404 NotFound if no posts are found.
        /// </returns>
        [HttpGet("get-by-me")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> GetPostByLoginAccount()
        {
            var userClaims = _authenService.GetDataFromToken();

            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var post = await _postService.GetPostsByAccId(userClaims.AccId);

            if (post?.Success == false)
                return NotFound(post);

            return Ok(post);
        }

        [HttpGet("account/{accId}")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> GetPostByAccount(string accId)
        {
            var userClaims = _authenService.GetDataFromToken();

            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var post = await _postService.GetPostsByAccId(accId);

            if (post?.Success == false)
                return NotFound(post);

            return Ok(post);
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

            //var userClaims = _authenService.GetDataFromToken();
            //var acc_id = userClaims?.AccId;

            var isDeletedSuccess = await _postService.DeletePost(request);

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
            if (userClaims == null)
                return Unauthorized("Not permission");

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
            if (userClaims == null)
                return Unauthorized("Not permission");

            var acc_id = userClaims?.AccId;

            var isDeletedSuccess = await _postService.RestorePostDeleted(acc_id, request);

            if (isDeletedSuccess == null)
                return BadRequest("Invalid data from request");

            if (isDeletedSuccess.Success == false)
                return NotFound(isDeletedSuccess);

            return Ok(isDeletedSuccess);
        }

        //[HttpGet("search-posts-in-group/{groupId}")]
        //public async Task<IActionResult> SearchPostsInGroup(string groupId, [FromQuery] string keyword)
        //{
        //    if (string.IsNullOrWhiteSpace(keyword))
        //        return BadRequest("Keyword is required.");

        //    var posts = await _postService.SearchPostsInGroupAsync(groupId, keyword);

        //    if (posts.Count == 0)
        //        return NotFound("No found posts.");

        //    return Ok(posts);
        //}

        [HttpGet("search-posts-in-group-dto/{groupId}")]
        public async Task<IActionResult> SearchPostsInGroupDTO(string groupId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var response = await _postService.SearchPostsWithAccountAsync(groupId, keyword);

            return Ok(response);
        }
        [HttpGet("count-post-in-group")]
        public async Task<IActionResult> CountPostInGroupDetail([FromQuery] string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return BadRequest(new { success = false, message = "groupId is required." });

            var count = await _postService.CountPublicPostsInGroupAsync(groupId);

            return Ok(new { success = true, data = count });
        }


        [HttpGet("list-valid")]
        public async Task<ActionResult<ListPostResponseDTO>> ListPostValid()
        {
            var result = await _postService.GetListPostValid();

            if(result == null || result.Success == false) 
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list-invalid")]
        public async Task<ActionResult<ListPostResponseDTO>> ListPostInvalid()
        {
            var result = await _postService.GetListPostDeleted();

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list-all")]
        public async Task<ActionResult<ListPostResponseDTO>> ListAllPost()
        {
            var result = await _postService.GetListAllPost();

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("infinite")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> ListPostInfinite([FromQuery] string? lastPostId, [FromQuery] int pageSize)
        {
            if (pageSize <= 0 || pageSize > 50)
                pageSize = 5; // default hoặc giới hạn max

            var result = await _postService.GetListInfinitePost(lastPostId, pageSize);

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("infinite-with-share")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> ListPostAndSharePostInfinite([FromQuery] string? lastPostId, [FromQuery] string? lastSharePostId, [FromQuery] int pageSize)
        {
            if (pageSize <= 0 || pageSize > 50)
                pageSize = 5; // default hoặc giới hạn max

            var result = await _postService.GetListInfinitePostAndSharePost(lastPostId, lastSharePostId, pageSize);

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list-checked-by-ai")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO?>> GetListPostCheckedAI()
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;
            if (acc_id == null)
                return Unauthorized();
            var result = await _postService.GetListPostCheckedAI();

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list-post-for-admin")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO?>> GetAllPostsForAdmin()
        {
            var result = await _postService.GetAllPostsForAdmin();

            if (result == null || result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("saved-post/{postId}")]
        [Authorize]
        public async Task<ActionResult<CreateSavedPostRequestDTO>> SavedPost([FromRoute] string? postId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;
            if(acc_id == null)
                return Unauthorized();

            var result = await _savedPostService.SavedPost(acc_id, postId);
            if (result == null)
                return BadRequest();

            if(result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("check-saved/{postId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckSavedPost([FromRoute] string? postId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;
            if (acc_id == null)
                return Unauthorized();

            var result = await _savedPostService.CheckIsSavedPost(acc_id, postId);
            return Ok(result);
        }

        [HttpDelete("unsaved/{postId}")]
        [Authorize]
        public async Task<ActionResult<bool>> UnsavedPost([FromRoute] string? postId)
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return Unauthorized("Not permission.");
            var acc_id = userClaims?.AccId;
            if (acc_id == null)
                return Unauthorized("Not permission."); ;

            var result = await _savedPostService.UnsavedPost(acc_id, postId);
            return Ok(result);
        }

        [HttpGet("list-saved")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> GetListSavedPostByAccount()
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;

            if(acc_id == null)
                return Unauthorized();

            var result = await _savedPostService.ListSavedPostOfAccount(acc_id);

            if (result == null)
                return BadRequest();

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("moderation-post/{postId}")]
        [Authorize]
        public async Task<ActionResult<bool?>> ModerationPostByAI(string postId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;

            if (acc_id == null)
                return Unauthorized();

            var result = await _postService.CheckPostByAI(postId);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("moderation")]
        
        public async Task<ActionResult<bool?>> ModerationContentByAI(string content)
        {

            var result = await _cohereService.IsAgricultureRelatedAsync(content);

            return Ok(result);
        }

        [HttpGet("account/deleted-post")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> ListDeletedPostByAccount()
        {
            var userClaims = _authenService.GetDataFromToken();

            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var result = await _postService.GetListDeletedPostByAccount(userClaims.AccId);

            if(result == null)
                return NotFound();

            if (result.Success == false)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("account/deleted-post-with-share")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> ListDeletedPostAndShareByAccount()
        {
            var userClaims = _authenService.GetDataFromToken();

            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var result = await _postService.GetListDeletedPostAndShareByAccount(userClaims.AccId);

            if (result == null)
                return NotFound();

            if (result.Success == false)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("self-view")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> GetPostSelfView()
        {
            var userClaims = _authenService.GetDataFromToken();

            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var result = await _postService.GetPostsOwner(userClaims.AccId);

            if (result == null)
                return NotFound();

            if (result.Success == false)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("another-view/{accId}")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> GetPostsPublicAnother([FromRoute] string? accId)
        {
            if(accId == null)
                return BadRequest("Don't have permission!");

            var result = await _postService.GetPostsPublicByAccId(accId);

            if (result == null)
                return NotFound();

            if (result.Success == false)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("self-view-with-share")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> GetPostaAndSharesSelfView()
        {
            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return BadRequest("Invalid data from request.");

            var result = await _postService.GetPostsOwnerWithShare(userClaims.AccId);
            if (result == null)
                return NotFound();
            if (result.Success == false)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("another-view-with-share/{accId}")]
        [Authorize]
        public async Task<ActionResult<ListPostResponseDTO>> GetPostsAndSharesPublicAnother([FromRoute] string? accId)
        {
            if (accId == null)
                return BadRequest("Don't have permission!");

            var result = await _postService.GetPostsPublicWithShareByAccId(accId);
            if (result == null)
                return NotFound();
            if (result.Success == false)
                return NotFound();
            return Ok(result);
        }

        //get list post in list group
        [HttpGet("get-post-in-user-groups")]
        [Authorize]
        public async Task<ActionResult<ListPostInGroupResponseDTO>> GetPostsInGroups([FromQuery] string? lastPostId, [FromQuery] int pageSize)
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;
            if (acc_id == null)
                return Unauthorized();
            if (pageSize <= 0 || pageSize > 50)
                pageSize = 5; // default hoặc giới hạn max

            var result = await _postService.GetPostsInYourGroups(lastPostId, pageSize, acc_id);
            return Ok(result);
        }

        //get list post in group detail
        [HttpGet("get-post-in-group-detail")]
        [Authorize]
        public async Task<ActionResult<ListPostInGroupResponseDTO>> GetPostsInGroupDetail([FromQuery] string? lastPostId, [FromQuery] int pageSize, [FromQuery] string groupId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var acc_id = userClaims?.AccId;
            if (acc_id == null)
                return Unauthorized();
            if (pageSize <= 0 || pageSize > 50)
                pageSize = 5; // default hoặc giới hạn max

            var result = await _postService.GetPostsInGroupDetail(lastPostId, pageSize, groupId);
            return Ok(result);
        }

        [HttpGet("images/{accId}")]
        public async Task<IActionResult> GetAllImagesOfOther(string accId)
        {
            if (string.IsNullOrEmpty(accId))
                return BadRequest("Account ID is required.");

            try
            {
                var imageUrls = await _postService.GetAllImage(accId);

                if (imageUrls == null || imageUrls.Count == 0)
                    return NotFound("No images found.");

                return Ok(new
                {
                    Success = true,
                    Count = imageUrls.Count,
                    Data = imageUrls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error.",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("images")]
        [Authorize]
        public async Task<IActionResult> GetAllImages()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null)
                return Unauthorized();
            if (string.IsNullOrEmpty(accId))
                return BadRequest("Account ID is required.");

            try
            {
                var imageUrls = await _postService.GetAllImage(accId);

                if (imageUrls == null || imageUrls.Count == 0)
                    return NotFound("No images found.");

                return Ok(new
                {
                    Success = true,
                    Count = imageUrls.Count,
                    Data = imageUrls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error.",
                    Error = ex.Message
                });
            }
        }
    }
}
