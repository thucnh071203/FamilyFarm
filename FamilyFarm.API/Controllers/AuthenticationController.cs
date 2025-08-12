using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
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
                return StatusCode(423, result); // Account locked
            }

            if (result != null)
            {
                return Ok(result); // ✅ Bao bọc trong Ok()
            }

            return Unauthorized();
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<LoginResponseDTO>> Logout()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;
            if (username == null)
                return BadRequest();

            var result = await _authenService.Logout(username);
            return result is not null ? Ok(result) : Unauthorized();
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
        public async Task<ActionResult<RegisterExpertReponseDTO>> RegisterExpert([FromForm] RegisterExpertRequestDTO request)
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

                    var content = $"<div><strong>Thank you for signing up to become an Expert. We will respond as soon as possible.<strong/></div>";
                    var html = EmailTemplateHelper.EmailConfirm(request.Email, content);

                    await _emailSender.SendEmailAsync(request.Email, "Register Expert", html);

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

        [Authorize]
        [HttpPut("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return BadRequest();

            var account = await _accountService.GetAccountById(userClaims.AccId);
            if (account == null)
                return NotFound("Account not found");

            if (account.Otp != -1)
                return BadRequest("Password has been set");

            account.PasswordHash = request.Password;
            account.Otp = null;
            await _accountService.UpdateAsync(userClaims.AccId, account);
            return Ok("Password setted successfully!");
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                var userClaims = _authenService.GetDataFromToken();
            if (userClaims == null)
                return BadRequest();

            var account = await _accountService.GetAccountById(userClaims.AccId);
            if (account == null)
                return NotFound("Account not found");

            if (!_hasher.VerifyPassword(request.OldPassword, account.PasswordHash))
                return BadRequest("Password wrong!");

            account.PasswordHash = request.NewPassword;
            account.Otp = null;
            await _accountService.UpdateAsync(userClaims.AccId, account);
            return Ok("Password changed successfully!");
        }

        [AllowAnonymous]
        [HttpPost("generate-OTP")]
        public async Task<IActionResult> GenerateOtp([FromForm] string id)
        {
            var account = await _accountService.GetAccountById(id);
            if (account == null)
            {
                return NotFound(new { Success = false, Message = "Account not found" });
            }

            if (account.Status != 0)
            {
                return BadRequest(new { Success = false, Message = "Account is inactive." });
            }

            // Generate OTP
            var random = new Random();
            int otpRandom = random.Next(100000, 999999);

            // Save OTP & Time
            account.Otp = otpRandom;
            account.CreateOtp = DateTime.UtcNow;

            await _accountService.UpdateOtpAsync(id, account);

            return Ok(new
            {
                Success = true,
                Message = "OTP generated successfully.",
                Otp = otpRandom // Nếu không muốn trả về client có thể bỏ dòng này
            });
        }

        [AllowAnonymous]
        [HttpPut("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = await _accountService.GetAccountById(request.AccId);
            if (account == null)
                return NotFound("Account not found");

            if (account.Status != 0)
            {
                return BadRequest("Account is inactivate.");
            }

            account.PasswordHash = request.Password;
            account.Otp = null;
            account.CreateOtp = null;
            await _accountService.UpdateAsync(request.AccId, account);
            return Ok("Password reset successfully!");
        }

        // Test send email 
        //[HttpPost("send-email")]
        //public async Task<IActionResult> SendEmail([FromBody] EmailRequestDTO request)
        //{
        //    var otp = 123456;
        //    var content = $"<p>Your OTP is:</p><div class='otp-box'>{otp}</div><p>It is valid for 2 minutes.</p>";
        //    var html = EmailTemplateHelper.EmailConfirm(request.ToEmail, content);

        //    await _emailSender.SendEmailAsync(request.ToEmail, request.Subject, html);

        //    return Ok("Email sent successfully!");
        //}
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDTO request)
        {
            // Get account by email
            var account = await _accountService.GetAccountByEmail(request.ToEmail);
            if (account == null)
            {
                return NotFound(new { Success = false, Message = "Account not found." });
            }

            if (account.Data.Otp == null || account.Data.CreateOtp == null)
            {
                return BadRequest(new { Success = false, Message = "OTP has not been generated yet." });
            }

            // Kiểm tra OTP hết hạn (giả sử 1 phút)
            if ((DateTime.UtcNow - account.Data.CreateOtp.Value).TotalMinutes > 1)
            {
                return BadRequest(new { Success = false, Message = "OTP has expired. Please request a new OTP." });
            }

            var otp = account.Data.Otp.Value;

            var content = $"<p>Your OTP is:</p><div class='otp-box'>{otp}</div><p>It is valid for 1 minutes.</p>";
            var html = EmailTemplateHelper.EmailConfirm(request.ToEmail, content);

            await _emailSender.SendEmailAsync(request.ToEmail, request.Subject, html);

            return Ok(new { Success = true, Message = "OTP email sent successfully." });
        }

    }
}
