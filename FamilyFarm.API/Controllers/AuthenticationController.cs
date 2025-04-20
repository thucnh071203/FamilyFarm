using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/authen")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenService;

        public AuthenticationController(IAuthenticationService authenService)
        {
            _authenService = authenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authenService.Login(request);

            if (result != null && result.MessageError != null)
            {
                return StatusCode(423, result);
            }

            return result is not null ? result : Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDTO>> Refresh([FromBody] RefreshTokenRequestDTO request)
        {
            if(string.IsNullOrEmpty(request.Token)) 
                return BadRequest("Invalid Token!");

            var result = await _authenService.ValidateRefreshToken(request.Token);

            return result is not null ? result : Unauthorized();
        }
    }
}
