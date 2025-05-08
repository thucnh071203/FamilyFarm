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

        /// <summary>
        /// Creates a new post by the authenticated user.
        /// </summary>
        /// <param name="request">The data required to create a new post, encapsulated in a SharePostRequestDTO object.</param>
        /// <returns>An ActionResult containing the result of the post creation,
        /// either a successful PostResponseDTO or an error response.</returns>
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> CreateSharePost([FromBody] SharePostRequestDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            var result = await _sharePostService.CreateSharePost(accId, request);

            if (result == null)
                return BadRequest(result);

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }
    }
}
