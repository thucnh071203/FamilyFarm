using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    // chua co post controller nen de tam reaction o day (co the chuyen qua post controller hoac khong)
    [Route("api/reaction")] // neu lam chung cho reaction-post và reaction-comment trong day (recommendation)
    [ApiController]
    public class ReactionController : ControllerBase
    {
        private readonly IReactionPostService _reactionPostService;

        public ReactionController(IReactionPostService reactionPostService)
        {
            _reactionPostService = reactionPostService;
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
        [HttpPost("all-by-post/{postId}")]
        public async Task<IActionResult> GetAllReactionByPost(string postId)
        {
            var result = await _reactionPostService.GetAllByPostAsync(postId);
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
        [HttpPost("toggle-reaction-post/{postId}")]
        public async Task<IActionResult> ToggleReactionPost(string postId, [FromBody] ReactionRequestDTO request)
        {
            var result = await _reactionPostService.ToggleReactionAsync(postId, request.AccId, request.CategoryReactionId);
            if (!result)
                return BadRequest("Reaction does not exist or is invalid.");

            return Ok("Reaction has been toggled.");
        }
    }
}
