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

        [HttpGet("get-all-by-post/{postId}")]
        public async Task<IActionResult> GetAllByPost(string postId) =>
            Ok(await _commentService.GetAllByPost(postId));

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var comment = await _commentService.GetById(id);
            return comment == null ? NotFound("Comment not found!") : Ok(comment);
        }

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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _commentService.Delete(id);
            return Ok("Delete successfully!");
        }
    }
}
