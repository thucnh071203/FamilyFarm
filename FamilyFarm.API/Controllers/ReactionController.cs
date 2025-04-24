using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    // chua co post nen de tam reaction o day 
    [Route("api/reaction")]
    [ApiController]
    public class ReactionController : ControllerBase
    {
        private readonly IReactionPostService _reactionPostService;

        public ReactionController(IReactionPostService reactionPostService)
        {
            _reactionPostService = reactionPostService;
        }

        [HttpPost("get-all-by-post/{postId}")]
        public async Task<IActionResult> GetAllReactionByPost(string postId)
        {
            var result = await _reactionPostService.GetAllByPostAsync(postId);
            return Ok(result);
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleReaction([FromBody] ReactionRequestDTO request)
        {
            var result = await _reactionPostService.ToggleReactionAsync(request.PostId, request.AccId, request.CategoryReactionId);
            if (!result)
                return BadRequest("Reaction does not exist or is invalid.");

            return Ok("Reaction has been toggled.");
        }
    }
}
