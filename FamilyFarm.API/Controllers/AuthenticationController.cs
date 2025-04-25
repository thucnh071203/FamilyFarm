using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace FamilyFarm.API.Controllers
{
    [Route("api/authen")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenService;
        private readonly IAccountService _accountService;
        private readonly PasswordHasher _hasher;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(IAuthenticationService authenService, IAccountService accountService, PasswordHasher hasher, IEmailSender emailSender)
        {
            _authenService = authenService;
            _accountService = accountService;
            _hasher = hasher;
            _emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authenService.Login(request);

            if (result != null && result.Message != null)
            {
                return StatusCode(423, result);
            }

            return result is not null ? result : Unauthorized();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<LoginResponseDTO>> Logout()
        {
            var username = _authenService.GetDataFromToken();

            if (username == null)
                return BadRequest();

            var result = await _authenService.Logout(username);
            return result is not null ? result : Unauthorized();
        }
        
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDTO>> Refresh([FromBody] RefreshTokenRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest("Invalid Token!");

            var result = await _authenService.ValidateRefreshToken(request.Token);

            return result is not null ? result : Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register-expert")]
        public async Task<ActionResult<RegisterExpertReponseDTO>> RegisterExpert([FromBody] RegisterExpertRequestDTO request)
        {
            if (ModelState.IsValid)
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
                if (listError.Count != 0)
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


        [HttpPost("register-farmer")]
        public async Task<ActionResult<RegisterFarmerResponseDTO>> RegisterFarmer([FromBody] RegisterFarmerRequestDTO request)
        {
            if (ModelState.IsValid)
            {
                List<string> listError = new List<string>();

                if (!_authenService.CheckValidEmail(request.Email))
                {
                    listError.Add("This email is invalid!");
                }
                if (!_authenService.CheckValidPhoneNumber(request.Phone))
                {
                    listError.Add("This phone is invalid!");
                }

                if (listError.Count != 0)
                {
                    return StatusCode(400, listError);
                }

                else
                {
                    var result = await _authenService.RegisterFarmer(request);

                    if (result == null)
                    {
                        return StatusCode(500, "Internal server error during registration.");
                    }

                    if (!result.IsSuccess)
                    {
                   
                        return StatusCode(400, result);
                    }
                          return Ok(result);
                }
            }
            else
            {
                return BadRequest(request);
                            }
        }

        [HttpPost("login-facebook")]
        public async Task<ActionResult<LoginResponseDTO>> LoginFacebook([FromBody] LoginFacebookRequestDTO request)
        {
            var result = await _authenService.LoginFacebook(request);

            if (result != null && result.Message != null)
            {
                return StatusCode(423, result);
            }

            return result is not null ? result : Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("login-google")]
        public async Task<ActionResult<LoginResponseDTO>> LoginGoogle([FromBody] LoginGoogleRequestDTO request)
        {
            var result = await _authenService.LoginWithGoogle(request);

            if (result != null && result.Message != null)
            {
                return StatusCode(423, result);
            }

            return result is not null ? result : Unauthorized();
        }

        [AllowAnonymous]
        [HttpPut("set-password/{id}")]
        public async Task<IActionResult> SetPassword(string id, [FromBody] SetPasswordDTO request)
        {
            var account = await _accountService.GetAccountById(id);
            if (account == null)
                return NotFound("Account not found");

            if (account.Otp != -1)
                return BadRequest("Password has been set");

            account.PasswordHash = request.Password;
            account.Otp = null;
            await _accountService.UpdateAsync(id, account);
            return Ok("Password setted successfully!");
        }

        [AllowAnonymous]
        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDTO request)
        {
            var account = await _accountService.GetAccountById(id);
            if (account == null)
                return NotFound("Account not found");

            if (!_hasher.VerifyPassword(request.OldPassword, account.PasswordHash))
                return BadRequest("Password wrong!");

            account.PasswordHash = request.NewPassword;
            account.Otp = null;
            await _accountService.UpdateAsync(id, account);
            return Ok("Password changed successfully!");
        }

        [AllowAnonymous]
        [HttpPut("generate-OTP/{id}")]
        public async Task<IActionResult> GenerateOtp(string id, [FromBody] GenerateOtpDTO request)
        {
            var account = await _accountService.GetAccountById(id);
            if (account == null)
            {
                return NotFound("Account not found");
            }
            
            if (account.Status != 0)
            {
                return BadRequest("Account is inactivate.");
            }

            account.Otp = request.Otp;
            await _accountService.UpdateOtpAsync(id, account);
            return Ok(new
            {
                message = "OTP updated successfully.",
                otp = request.Otp
            });
        }

        [AllowAnonymous]
        [HttpPut("forgot-password/{id}")]
        public async Task<IActionResult> ForfotPassword(string id, [FromBody] ResetPasswordDTO request)
        {
            var account = await _accountService.GetAccountById(id);
            if (account == null)
                return NotFound("Account not found");

            if (account.Status != 0)
            {
                return BadRequest("Account is inactivate.");
            } else if (account.Otp != request.Otp)
            {
                return BadRequest("Otp does not match.");
            }

            account.PasswordHash = request.Password;
            account.Otp = null;
            await _accountService.UpdateAsync(id, account);
            return Ok("Password reset successfully!");
        }

        // Test send email 
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDTO request)
        {
            var otp = 123456; 
            var content = $"<p>Your OTP is:</p><div class='otp-box'>{otp}</div><p>It is valid for 2 minutes.</p>";
            var html = EmailTemplateHelper.EmailConfirm(request.ToEmail, content);

            await _emailSender.SendEmailAsync(request.ToEmail, request.Subject, html);

            return Ok("Email sent successfully!");
        }
    }
}
