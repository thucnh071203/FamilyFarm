using AutoMapper;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FamilyFarm.API.Controllers
{
    [Route("api/comment")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IAuthenticationService _authenService;

        public CommentController(ICommentService commentService, IAuthenticationService authenService)
        {
            _commentService = commentService;
            _authenService = authenService;
        }

        /// <summary>
        /// Retrieves all comments associated with a specific post.
        /// This endpoint fetches all the comments related to the given post ID.
        /// </summary>
        /// <param name="postId">The unique identifier for the post whose comments need to be retrieved.</param>
        /// <returns>
        /// An IActionResult containing a list of comments for the specified post.
        /// - 200 OK with the list of comments if comments are retrieved successfully or no comments exist.
        /// - 400 BadRequest if the post ID is invalid.
        /// </returns>
        [Authorize]
        [HttpGet("all-by-post/{postId}")]
        public async Task<IActionResult> GetListCommentOfPost(string? postId)
        {
            if (postId == null)
                return BadRequest();

            var result = await _commentService.GetAllCommentWithReactionByPost(postId);

            if (result == null)
                return BadRequest();

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a comment by its unique identifier.
        /// This endpoint fetches a comment based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to retrieve.</param>
        /// <returns>
        /// An IActionResult containing a CommentResponseDTO:
        /// - Success: true, Data: comment, 200 OK if the comment is found.
        /// - Success: false, Message: error message, 404 NotFound if the comment does not exist.
        /// </returns>
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await _commentService.GetById(id);
            return response.Success.GetValueOrDefault() ? Ok(response) : NotFound(response);
        }

        /// <summary>
        /// Creates a new comment for a post.
        /// This endpoint allows the authenticated user to submit a new comment.
        /// </summary>
        /// <param name="request">The CommentRequestDTO containing the post ID and comment content.</param>
        /// <returns>
        /// An IActionResult containing a CommentResponseDTO:
        /// - Success: true, Data: created comment, 200 OK if the operation is successful.
        /// - Success: false, Message: error message, 400 BadRequest if the request is invalid or authentication fails.
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CommentRequestDTO request)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return BadRequest(new CommentResponseDTO { Success = false, Message = "Please Login!" });

            var response = await _commentService.Create(request, account.AccId);
            return response.Success.GetValueOrDefault() ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Updates an existing comment.
        /// This endpoint allows the authenticated user to modify an existing comment's content.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to update.</param>
        /// <param name="request">The CommentRequestDTO containing the updated comment content.</param>
        /// <returns>
        /// An IActionResult containing a CommentResponseDTO:
        /// - Success: true, Data: updated comment, 200 OK if the update is successful.
        /// - Success: false, Message: error message, 404 NotFound if the comment does not exist or authentication fails.
        /// </returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CommentRequestDTO request)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return BadRequest(new CommentResponseDTO { Success = false, Message = "Please Login!" });

            var response = await _commentService.Update(id, request, account.AccId);
            if (response.Success.GetValueOrDefault())
                return Ok(response);
            if (response.Message == "Invalid comment data")
                return BadRequest(response);
            return NotFound(response);
        }

        /// <summary>
        /// Deletes a comment by its unique identifier.
        /// This endpoint allows the authenticated user to delete a specific comment.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to delete.</param>
        /// <returns>
        /// An IActionResult containing a CommentResponseDTO:
        /// - Success: true, Message: success message, 200 OK if the comment is successfully deleted.
        /// - Success: false, Message: error message, 404 NotFound if the comment does not exist or authentication fails.
        /// </returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return BadRequest(new CommentResponseDTO { Success = false, Message = "Please Login!" });

            var response = await _commentService.Delete(id, account.AccId);
            if (response.Success.GetValueOrDefault())
                return Ok(response);
            if (response.Message == "Invalid Comment ID")
                return BadRequest(response);
            return NotFound(response);
        }
    }
}