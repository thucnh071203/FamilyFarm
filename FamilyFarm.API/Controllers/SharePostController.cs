using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/share-post")]
    [ApiController]
    public class SharePostController : ControllerBase
    {
        private readonly ISharePostService _sharePostService;
        private readonly IAuthenticationService _authenService;

        public SharePostController(ISharePostService sharePostService, IAuthenticationService authenService)
        {
            _sharePostService = sharePostService;
            _authenService = authenService;
        }

        [HttpPost("account/{accId}")]
        [Authorize]
        public async Task<ActionResult<SharePostResponseDTO>> GetSharePostsByAccId(string? accId)
        {
            //var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.GetSharePostsByAccId(accId);
            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("post/{postId}")]
        [Authorize]
        public async Task<ActionResult<ListSharePostResponseDTO>> GetSharePostsByPostId(string? postId)
        {
            //var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.GetSharePostsByPostId(postId);
            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("get-by-me")]
        [Authorize]
        public async Task<ActionResult<SharePostResponseDTO>> GetSharePostsByMe()
        {
            var userClaims = _authenService.GetDataFromToken();
            var result = await _sharePostService.GetSharePostsByAccId(userClaims?.AccId);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Creates a new post by the authenticated user.
        /// </summary>
        /// <param name="request">The data required to create a new post, encapsulated in a SharePostRequestDTO object.</param>
        /// <returns>An ActionResult containing the result of the post creation,
        /// either a successful PostResponseDTO or an error response.</returns>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> CreateSharePost([FromForm] SharePostRequestDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.CreateSharePost(userClaims?.AccId, request);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("update/{sharePostId}")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> UpdateSharePost(string sharePostId, [FromForm] UpdateSharePostRequestDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.UpdateSharePost(sharePostId, request);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("hard-delete/{sharePostId}")]
       // [Authorize]
        public async Task<ActionResult<PostResponseDTO>> HardDeleteSharePost(string sharePostId)
        {
            var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.HardDeleteSharePost(sharePostId);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("soft-delete/{sharePostId}")]
        // [Authorize]
        public async Task<ActionResult<PostResponseDTO>> SoftDeleteSharePost(string sharePostId)
        {
            var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.SoftDeleteSharePost(sharePostId);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("restore/{sharePostId}")]
        public async Task<ActionResult<PostResponseDTO>> RestoreSharePost(string sharePostId)
        {
            var userClaims = _authenService.GetDataFromToken();

            var result = await _sharePostService.RestoreSharePost(sharePostId);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }
    }
}
