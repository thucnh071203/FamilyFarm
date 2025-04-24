using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [AllowAnonymous]
        [HttpPost("registerExpert")]
        public async Task<ActionResult<RegisterExpertReponseDTO>> RegisterExpert([FromBody] RegisterExpertRequestDTO request)
        {
            if(ModelState.IsValid)
            {
                List<string> listError = new List<string>();

                if (!_authenService.IsValidEmail(request.Email))
                {
                    listError.Add("This email is invalid!");
                }
                if (!_authenService.IsValidPhoneNumber(request.Phone))
                {
                    listError.Add("This phone is invalid!");
                }
                if (!_authenService.IsValidIdentifierNumber(request.Identifier))
                {
                    listError.Add("This identify number is invalid!");
                }
                if(listError.Count !=0)
                {
                    return StatusCode(423, listError);
                }
                else
                {
                    var result = await _authenService.RegisterExpert(request);

                    if (result == null)
                    {
                        // Lỗi không xác định – ví dụ: service trả về null không rõ lý do
                        return StatusCode(500, "Internal server error during registration.");
                    }

                    if (!result.IsSuccess)
                    {
                        // Trả về mã 423 hoặc có thể là 400 nếu dữ liệu sai
                        return StatusCode(423, result);
                    }
                    // Thành công
                    return Ok(result);
                }


            }
            else
            {
                return BadRequest(request);
            }
          
            

           
        }
    }
}
