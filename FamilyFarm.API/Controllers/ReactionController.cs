using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/reaction")] // lam chung cho reaction-post và reaction-comment trong day (recommendation)
    [ApiController]
    public class ReactionController : ControllerBase
    {
        private readonly IReactionService _reactionService;
        private readonly IAuthenticationService _authenService;
        public ReactionController(IReactionService reactionService, IAuthenticationService authenService)
        {
            _reactionService = reactionService;
            _authenService = authenService;
        }

        /// <summary>
        /// Retrieves all reactions associated with a specific post.
        /// This endpoint fetches the reactions given by users for the given post.
        /// </summary>
        /// <param name="postId">The unique identifier for the post whose reactions need to be retrieved.</param>
        /// <returns>
        /// An IActionResult containing the list of reactions for the specified post.
        /// If reactions exist, returns them with a 200 OK status.
        /// If no reactions are found, it will still return an empty list with a 200 OK status.
        /// </returns>
        //[Authorize]
        [HttpGet("all-by-post/{postId}")]
        public async Task<IActionResult> GetAllReactionsByPost(string postId)
        {
            var result = await _reactionService.GetAllByEntityAsync(postId, "Post");
            return Ok(result);
        }

        /// <summary>
        /// Toggles a reaction for a given post by a specific user.
        /// This endpoint either adds or removes the user's reaction based on its current state.
        /// </summary>
        /// <param name="postId">The unique identifier for the post to which the reaction is related.</param>
        /// <param name="request">The request containing the account ID (AccId) and the reaction category ID (CategoryReactionId).</param>
        /// <returns>
        /// An IActionResult indicating the outcome of the toggle operation:
        /// - 200 OK with a success message if the reaction was successfully toggled,
        /// - 400 BadRequest if the reaction does not exist or is invalid (e.g., invalid category or account ID).
        /// </returns>
        [Authorize]
        [HttpPost("toggle-post/{postId}")]
        public async Task<IActionResult> ToggleReactionPost(string postId, [FromQuery] string categoryReactionId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return NotFound("No account found!");

            var result = await _reactionService.ToggleReactionAsync(postId, "Post", account.AccId, categoryReactionId);
            if (!result)
                return BadRequest("Reaction does not exist or is invalid.");

            return Ok("Reaction has been toggled.");
        }

        /// <summary>
        /// Retrieves all reactions associated with a specific comment.
        /// This endpoint fetches the reactions given by users for the given comment.
        /// </summary>
        /// <param name="commentId">The unique identifier for the comment whose reactions need to be retrieved.</param>
        /// <returns>
        /// An IActionResult containing the list of reactions for the specified comment.
        /// If reactions exist, returns them with a 200 OK status.
        /// If no reactions are found, it will still return an empty list with a 200 OK status.
        /// </returns>
        //[Authorize]
        [HttpGet("all-by-comment/{commentId}")]
        public async Task<IActionResult> GetAllReactionsByComment(string commentId)
        {
            var result = await _reactionService.GetAllByEntityAsync(commentId, "Comment");
            return Ok(result);
        }

        /// <summary>
        /// Toggles a reaction for a given comment by a specific user.
        /// This endpoint either adds or removes the user's reaction based on its current state.
        /// </summary>
        /// <param name="commentId">The unique identifier for the comment to which the reaction is related.</param>
        /// <param name="request">The request containing the account ID (AccId) and the reaction category ID (CategoryReactionId).</param>
        /// <returns>
        /// An IActionResult indicating the outcome of the toggle operation:
        /// - 200 OK with a success message if the reaction was successfully toggled,
        /// - 400 BadRequest if the reaction does not exist or is invalid (e.g., invalid category or account ID).
        /// </returns>
        [Authorize]
        [HttpPost("toggle-comment/{commentId}")]
        public async Task<IActionResult> ToggleReactionComment(string commentId, [FromQuery] string categoryReactionId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return NotFound("No account found!");

            var result = await _reactionService.ToggleReactionAsync(commentId, "Comment", account.AccId, categoryReactionId);
            if (!result)
                return BadRequest("Reaction does not exist or is invalid.");

            return Ok("Reaction has been toggled.");
        }
    }
}
