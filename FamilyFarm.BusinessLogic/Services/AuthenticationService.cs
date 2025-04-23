using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace FamilyFarm.BusinessLogic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHasher _hasher;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationService(IConfiguration configuration, IAccountRepository accountRepository, PasswordHasher hasher, TokenValidationParameters tokenValidationParameters)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _hasher = hasher;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<LoginResponseDTO?> Login(LoginRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Identifier) || string.IsNullOrEmpty(request.Password))
                return null;

            var account = await _accountRepository.GetAccountByIdentifier(request.Identifier);

            //KIỂM TRA account có hay không
            if (account == null)
            {
                return null;
            }

            //KIỂM TRA XEM TÀI KHOẢN CÓ BỊ KHÓA LOGIN HAY KHÔNG
            if (account.LockedUntil != null && account.LockedUntil > DateTime.UtcNow)
            {
                return new LoginResponseDTO
                {
                    MessageError = "Account is locked login.",
                    LockedUntil = account.LockedUntil
                };
            }

            //Kiểm tra xem có đúng password hay không
            if (!_hasher.VerifyPassword(request.Password, account.PasswordHash))
            {
                //Kiểm tra xem trường FailedAttempts có bị null hay không
                int failNumb = account.FailedAttempts ?? 0;
                failNumb++;

                if (account.FailedAttempts >= 5)
                {
                    //Reset lại số lần thất bại thành 0 và cập nhật thời gian khóa mới
                    var lockedUntil = DateTime.UtcNow.AddSeconds(10);
                    await _accountRepository.UpdateLoginFail(account.AccId, 0, lockedUntil);

                    return new LoginResponseDTO
                    {
                        MessageError = "Account is locked login.",
                        LockedUntil = lockedUntil
                    };
                } 
                else
                {
                    await _accountRepository.UpdateLoginFail(account.AccId, failNumb, null);
                    return null;
                }
            }

            // Reset số lần fail nếu đăng nhập thành công
            await _accountRepository.UpdateLoginFail(account.AccId, 0, null);

            return await GenerateToken(account);
        }

        public async Task<LoginResponseDTO> LoginFacebook(LoginFacebookRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.FacebookId))
                return null;
                
            var account = await _accountRepository.GetByFacebookId(request.FacebookId);

            // Nếu chưa có tài khoản thì tạo mới tài khoản mới
            if (account == null)
            {
                await _accountRepository.CreateFacebookAccount(request.FacebookId, request.Name, request.Email, request.Avatar);

                var acountRegistered = await _accountRepository.GetByFacebookId(request.FacebookId);

                return await GenerateToken(acountRegistered);
            }

            return await GenerateToken(account);
        }

        public async Task<LoginResponseDTO?> ValidateRefreshToken(string refreshToken)
        {
            //Lấy account dựa trên refreshToken
            var account = await _accountRepository.GetAccountByRefreshToken(refreshToken);

            //Nếu refreshToken không có hoặc có nhưng hết hạn thì return null
            if(account == null || account.TokenExpiry < DateTime.UtcNow) return null;

            //Reset value 2 field RefreshToken và Expiry trước khi tạo mới
            await _accountRepository.UpdateRefreshToken(account.AccId, null, null);

            //Gọi GenerateToken để tạo accessToken và refreshToken mới
            return await GenerateToken(account);
        } 

        private async Task<LoginResponseDTO> GenerateToken(Account account)
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var tokenValidityMins = _configuration.GetValue<int>("JwtSettings:TokenValidMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var token = new JwtSecurityToken(issuer,
                audience, [
                    new Claim(JwtRegisteredClaimNames.Name, account.Username)
                    ],
                expires: tokenExpiryTimeStamp,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature));

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDTO
            {
                Username = account.Username,
                AccessToken = accessToken,
                TokenExpiryIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
                RefreshToken = await GenerateRefreshToken(account)
            };
        }

        private async Task<string?> GenerateRefreshToken(Account account)
        {
            var refreshTokenValidMins = _configuration.GetValue<int>("JwtSettings:RefreshTokenValidMins");

            if (account == null) return null;

            string newRefreshToken = Guid.NewGuid().ToString();
            DateTime newExpiry = DateTime.UtcNow.AddMinutes(refreshTokenValidMins);

            await _accountRepository.UpdateRefreshToken(account.AccId, newRefreshToken, newExpiry);

            return newRefreshToken;
        }

        public string? GetDataFromToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out _);

                var username = principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

                return username;
            } catch
            {
                return null;
            }
        }



        public async Task<RegisterFarmerResponseDTO> RegisterFarmer(RegisterFarmerRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone))
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Information is not null."
                };
            }

            var existingAccount = await _accountRepository.GetAccountByIdentifier(request.Username);
            if (existingAccount != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Username is already."
                };
            }

            var existingEmail = await _accountRepository.GetAccountByEmail(request.Email);
            if (existingEmail != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Email is already."
                };
            }

            var hashedPassword = _hasher.HashPassword(request.Password);

            var newAccount = new Account
            {
                AccId = ObjectId.GenerateNewId().ToString(),
                RoleId = "68007b0387b41211f0af1d56", 
                Username = request.Username,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                Gender = null, 
                City = request.City,
                Country = request.Country,
                Status = 1,
                FailedAttempts = 0,
                LockedUntil = null,
                TokenExpiry = null,
                RefreshToken = null,
                CreateOtp = null
            };

            var createdAccount = await _accountRepository.CreateFarmer(newAccount);
            if (createdAccount != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = true,
                    MessageError = null
                };
            }

            return new RegisterFarmerResponseDTO
            {
                IsSuccess = false,
                MessageError = "Register fail!."
            };
        }




        public async Task<LoginResponseDTO?> LoginWithGoogle(LoginGoogleRequestDTO request)
        {
            var account = await _accountRepository.GetAccountByEmail(request.Email);
            if (account == null)
            {
                // Tạo tài khoản mới nếu chưa tồn tại
                account = new Account
                {
                    AccId = ObjectId.GenerateNewId().ToString(),
                    Username = request.Email,
                    PasswordHash = "",
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = "",
                    Birthday = null,
                    Gender = "Not specified",
                    City = "",
                    Country = "",
                    Status = 0,
                    Otp = -1,
                    RoleId = "68007b0387b41211f0af1d56", // Mặc định là FARMER
                };
                await _accountRepository.CreateAsync(account);
            }

            else if (account.LockedUntil != null && account.LockedUntil > DateTime.UtcNow)
            {
                return new LoginResponseDTO
                {
                    MessageError = "Account is locked login.",
                    LockedUntil = account.LockedUntil
                };
            }

            return await GenerateToken(account);
        }

    }
}
