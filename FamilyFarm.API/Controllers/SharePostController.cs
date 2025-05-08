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

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<PostResponseDTO>> CreateNewPost([FromForm] SharePostRequestDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;

            var result = await _sharePostService.CreateSharePost(accId, request);

            if (result == null)
                return BadRequest();

            if (result.Success == false)
                return NotFound(result);

            return Ok(result);
        }
    }
}
