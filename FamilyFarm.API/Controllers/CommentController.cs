using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Retrieves all comments associated with a specific post.
        /// This endpoint fetches all the comments related to the given post ID.
        /// </summary>
        /// <param name="postId">The unique identifier for the post whose comments need to be retrieved.</param>
        /// <returns>
        /// An IActionResult containing the list of comments for the specified post.
        /// If comments exist, returns them with a 200 OK status.
        /// If no comments exist for the post, it returns an empty list with a 200 OK status.
        /// </returns>
        [HttpGet("all-by-post/{postId}")]
        public async Task<IActionResult> GetAllByPost(string postId)
        {
            var comments = await _commentService.GetAllByPost(postId);
            return Ok(comments);
        }

        /// <summary>
        /// Retrieves a comment by its unique identifier.
        /// This endpoint fetches a comment based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to retrieve.</param>
        /// <returns>
        /// An IActionResult containing the requested comment if found, or a 404 NotFound status if the comment does not exist.
        /// </returns>
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var comment = await _commentService.GetById(id);
            return comment == null ? NotFound("Comment not found!") : Ok(comment);
        }

        /// <summary>
        /// Creates a new comment.
        /// This endpoint allows the user to submit a new comment for a post.
        /// </summary>
        /// <param name="comment">The comment object containing the details to be saved.</param>
        /// <returns>
        /// An IActionResult indicating the outcome of the create operation:
        /// - 200 OK with the newly created comment if the operation is successful.
        /// - 400 BadRequest if the provided comment object is invalid or has missing fields (e.g., invalid PostId or AccId).
        /// </returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Comment comment)
        {
            if (comment == null)
                return BadRequest("Comment object is null");

            var result = await _commentService.Create(comment);
            if (result == null)
                return BadRequest("Invalid PostId or AccId");

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing comment.
        /// This endpoint allows users to modify an existing comment's content.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to update.</param>
        /// <param name="comment">The updated comment object containing the new content.</param>
        /// <returns>
        /// An IActionResult indicating the outcome of the update operation:
        /// - 200 OK with the updated comment if the update is successful.
        /// - 404 NotFound if the comment with the specified ID does not exist.
        /// </returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(string id, Comment comment)
        {
            var existing = await _commentService.GetById(id);
            if (existing == null)
                return NotFound("Comment not found!");

            existing.Content = comment.Content;
            comment = await _commentService.Update(id, existing);
            return Ok(comment);
        }

        /// <summary>
        /// Deletes a comment by its unique identifier.
        /// This endpoint allows users to delete a specific comment.
        /// </summary>
        /// <param name="id">The unique identifier of the comment to delete.</param>
        /// <returns>
        /// An IActionResult indicating the outcome of the delete operation:
        /// - 200 OK with a success message if the comment is successfully deleted.
        /// </returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _commentService.Delete(id);
            return Ok("Delete successfully!");
        }
    }
}
