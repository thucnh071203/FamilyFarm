using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyFarm.BusinessLogic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHasher _hasher;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private readonly IUploadFileService _uploadFileService;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        private readonly IStatisticRepository _statisticRepository;
        public AuthenticationService(IConfiguration configuration, IAccountRepository accountRepository, PasswordHasher hasher, TokenValidationParameters tokenValidationParameters, IHttpContextAccessor httpContextAccessor, IRoleRepository roleRepository, IUploadFileService uploadFileService, IHubContext<TopEngagedPostHub> hubContext, IStatisticRepository statisticRepository)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _hasher = hasher;
            _tokenValidationParameters = tokenValidationParameters;
            _httpContextAccessor = httpContextAccessor;
            _roleRepository = roleRepository;
            _uploadFileService = uploadFileService;
            _hubContext = hubContext;
            _statisticRepository = statisticRepository;
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


            //KIỂM TRA XEM ACCOUNT CÓ BỊ DELETE HAY KHÔNG
            if(account.Status == 1)
            {
                return new LoginResponseDTO
                {
                    Message = "Account does not exist or has been deleted."
                };
            }
            //KIỂM TRA XEM TÀI KHOẢN CÓ BỊ KHÓA LOGIN HAY KHÔNG
            if (account.LockedUntil != null && account.LockedUntil > DateTime.UtcNow)
            {
                return new LoginResponseDTO
                {
                    Message = "Account is locked login.",
                    LockedUntil = account.LockedUntil
                };
            }

            //KIỂM TRA XEM TÀI KHOẢN ĐÓ ROLE EXPERT CÓ ĐƯỢC ACCEPT HAY CHƯA
            if(account.RoleId == "68007b2a87b41211f0af1d57")
            {
                if(account.Status == 2)
                {
                    return new LoginResponseDTO
                    {
                        Message = "Your account has not been approved."
                    };
                }
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
                    var lockedUntil = DateTime.UtcNow.AddMinutes(3);
                    await _accountRepository.UpdateLoginFail(account.AccId, 0, lockedUntil);

                    return new LoginResponseDTO
                    {
                        Message = "Your username or password is incorrect.",
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

        public async Task<LoginResponseDTO?> LoginFacebook(LoginFacebookRequestDTO request)
        {
            if (request == null)
            {
                return null;
            }

            var account = await _accountRepository.GetByFacebookId(request.FacebookId);

            // Nếu chưa có tài khoản thì tạo mới tài khoản mới
            if (account == null)
            {
                // Đăng nhập bằng email nếu email đã tồn tại
                var checkEmailExisted = await _accountRepository.GetAccountByEmail(request.Email);

                if (checkEmailExisted != null)
                {
                    return await GenerateToken(checkEmailExisted);
                }

                string? avatarUrlFirebase = null;
                var fileName = $"facebook_avatar_{request.FacebookId}.jpg";

                if (!string.IsNullOrWhiteSpace(request.Avatar))
                {
                    var stream = await _uploadFileService.DownloadImageFromUrl(request.Avatar);
                    if (stream != null)
                    {
                        var uploadResult = await _uploadFileService.UploadImageFromStream(stream, fileName);
                        avatarUrlFirebase = uploadResult.UrlFile;
                    }
                }

                await _accountRepository.CreateFacebookAccount(request.FacebookId, request.Name, request.Email, avatarUrlFirebase);

                var acountRegistered = await _accountRepository.GetByFacebookId(request.FacebookId);

                return await GenerateToken(acountRegistered);
            }

            if (account.LockedUntil != null && account.LockedUntil > DateTime.UtcNow)
            {
                return new LoginResponseDTO
                {
                    Message = "Account is locked login.",
                    LockedUntil = account.LockedUntil
                };
            }

            return await GenerateToken(account);
        }

        public async Task<LoginResponseDTO?> ValidateRefreshToken(string refreshToken)
        {
            //Lấy account dựa trên refreshToken
            var account = await _accountRepository.GetAccountByRefreshToken(refreshToken);

            //Nếu refreshToken không có hoặc có nhưng hết hạn thì return null
            if (account == null || account.TokenExpiry < DateTime.UtcNow) return null;

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

            var role = await _roleRepository.GetRoleById(account.RoleId);

            if (role == null)
            {
                role = new Role
                {
                    RoleId = "68007b0387b41211f0af1d56", // RoleId mặc định nếu không tìm thấy
                    RoleName = "FARMER",              // Role mặc định là "FARMER"
                    RoleDescription = "farmer"
                };
            }

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims: new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Name, account.Username),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim("AccId", account.AccId),
                    new Claim("RoleId", account.RoleId),
                    new Claim(ClaimTypes.Role, role.RoleName) // Sử dụng tên vai trò trong token
                },
                expires: tokenExpiryTimeStamp,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDTO
            {
                AccId = account.AccId,
                Username = account.Username,
                RoleId = account.RoleId,
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

        public UserClaimsResponseDTO? GetDataFromToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var accessToken = authHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out _);

                var claims = principal?.Claims;

                var userClaims = new UserClaimsResponseDTO
                {
                    Username = claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value,
                    Email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    AccId = claims?.FirstOrDefault(c => c.Type == "AccId")?.Value,
                    RoleId = claims?.FirstOrDefault(c => c.Type == "RoleId")?.Value,
                    RoleName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                };

                return userClaims;
            }
            catch
            {
                return null;
            }
        }



        public async Task<RegisterExpertReponseDTO?> RegisterExpert(RegisterExpertRequestDTO request)
        {
            if (request == null) return null;
            if (await IsCheckDuplicateUsername(request.Username) == true)
            {
                return new RegisterExpertReponseDTO
                {
                    IsSuccess = false,
                    MessageError = "This username is already existed!"
                };
            }
            else if (await IsCheckDuplicateEmail(request.Email) == true)
            {
                return new RegisterExpertReponseDTO
                {
                    IsSuccess = false,
                    MessageError = "This email is already existed!"
                };
            }
            else if (await IsCheckDuplicatePhone(request.Phone) == true)
            {
                return new RegisterExpertReponseDTO
                {
                    IsSuccess = false,
                    MessageError = "This phone number is already existed!"
                };
            }
            else if (await IsCheckDuplicateIdentifierNumber(request.Identifier) == true)
            {
                return new RegisterExpertReponseDTO
                {
                    IsSuccess = false,
                    MessageError = "This identifier number is already existed!"
                };
            }
            else
            {
                try
                {
                    var avatar = "https://firebasestorage.googleapis.com/v0/b/prn221-69738.appspot.com/o/image%2Fdefault-avatar.png?alt=media&token=3857a1ec-ace4-4329-8f36-4f3a40c3fc67";
                    var certificate = "";
                   

                    if (request.Avatar != null)
                    {
                        //Goi method upload List image tu Upload file service
                        var ImageUrl = await _uploadFileService.UploadImage(request.Avatar);

                        if (ImageUrl != null)
                        {
                            avatar = ImageUrl.UrlFile ?? "";
                            
                        }
                    }

                    if (certificate != null)
                    {
                        //Goi method upload List image tu Upload file service
                        var ImageUrl = await _uploadFileService.UploadImage(request.Certificate);

                        if (ImageUrl != null)
                        {
                            certificate = ImageUrl.UrlFile ?? "";

                        }
                    }


                    await _accountRepository.CreateAccount(new Account
                    {
                        AccId = "",
                        RoleId = "68007b2a87b41211f0af1d57",
                        Username = request.Username,
                        PasswordHash = _hasher.HashPassword(request.Password),
                        FullName = request.Fullname,
                        Email = request.Email,
                        PhoneNumber = request.Phone,
                        Birthday = request.Birthday,
                        Gender = request.Gender,
                        City = request.City,
                        Country = request.Country,
                        IdentifierNumber = request.Identifier,
                        Address = request.Address,
                        Avatar = avatar,
                        Background = null,
                        Certificate = certificate,
                        WorkAt = null,
                        StudyAt = null,
                        RefreshToken = null,
                        TokenExpiry = null,
                        FailedAttempts = 0,
                        LockedUntil = null,
                        Status = 2,// chờ admin duyệt
                        Otp = null,
                        CreateOtp = null,
                        CreatedAt = DateTime.UtcNow,


                    });
                    int farmerCount = await _accountRepository.CountAccountsByRole("68007b0387b41211f0af1d56");
                    int expertCount = await _accountRepository.CountAccountsByRole("68007b2a87b41211f0af1d57");

                    await _hubContext.Clients.All.SendAsync("UpdateAccountCount", new
                    {
                        farmerCount,
                        expertCount
                    });
                    var usersByProvince = await _statisticRepository.GetUsersByProvinceAsync();
                    await _hubContext.Clients.All.SendAsync("UsersByProvince", usersByProvince);


                    DateTime startDate = DateTime.Today.AddDays(-30);
                    DateTime endDate = DateTime.Today;

                    var userGrowth = await _accountRepository.GetUserGrowthOverTimeAsync(startDate, endDate);

                    await _hubContext.Clients.All.SendAsync("ReceiveUserGrowth", userGrowth);


                    return new RegisterExpertReponseDTO
                    {

                        IsSuccess = true,
                        MessageError = null
                    };
                }
                catch (Exception ex)
                {
                    return new RegisterExpertReponseDTO
                    {
                        IsSuccess = false,
                        MessageError = $"Lỗi hệ thống: {ex.Message}"
                    };
                }

            }
        }

        /// <summary>
        /// check duplicate username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>false mean not duplicate, true mean this username is duplicate </returns>
        public async Task<bool> IsCheckDuplicateUsername(string username)
        {
            var acc = await _accountRepository.GetAccountByUsername(username);
            if (acc == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// check duplicate email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>false mean not duplicate, true mean this email is duplicate </returns>
        public async Task<bool> IsCheckDuplicateEmail(string email)
        {
            var acc = await _accountRepository.GetAccountByEmail(email);
            if (acc == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// check dupliate phone number 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns>false mean not duplicate, true mean this phone number is duplicate</returns>
        public async Task<bool> IsCheckDuplicatePhone(string phone)
        {
            var acc = await _accountRepository.GetAccountByPhone(phone);
            if (acc == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// check duplicate identifier number
        /// </summary>
        /// <param name="identifierNumber"></param>
        /// <returns>false mean not duplicate, true mean this identifier number is duplicate</returns>
        public async Task<bool> IsCheckDuplicateIdentifierNumber(string identifierNumber)
        {
            var acc = await _accountRepository.GetAccountByIdentifierNumber(identifierNumber);
            if (acc == null)
            {
                return false;
            }
            return true;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var regex = new Regex(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        public bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var regex = new Regex(@"^(0|\+84)[3-9][0-9]{8}$");  // Ví dụ: 038xxx, 090xxx, +8498xxx
            return regex.IsMatch(phone);
        }

        public bool IsValidIdentifierNumber(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                return false;

            // CMND: 9 số | CCCD: 12 số
            var regex = new Regex(@"^\d{9}$|^\d{12}$");
            return regex.IsMatch(identityNumber);
        }

        public async Task<RegisterFarmerResponseDTO?> RegisterFarmer(RegisterFarmerRequestDTO request)
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
         
            var existingAccount = await _accountRepository.GetAccountByIdentifierNumber(request.Identify);
            if (existingAccount != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Identify is already."
                };
            }

            var existingPhone = await _accountRepository.GetAccountByPhone(request.Phone);
            if (existingPhone != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Phone is already."
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



            var existingUsername = await _accountRepository.GetAccountByUsernameU(request.Username);
            if (existingUsername != null)
            {
                return new RegisterFarmerResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Username is already."
                };
            }




            var hashedPassword = _hasher.HashPassword(request.Password);

            var newAccount = new Account
            {
                AccId = "",
                RoleId = "68007b0387b41211f0af1d56",
                Username = request.Username,
                Avatar = "https://firebasestorage.googleapis.com/v0/b/prn221-69738.appspot.com/o/image%2Fdefault-avatar.png?alt=media&token=3857a1ec-ace4-4329-8f36-4f3a40c3fc67",
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                Gender = null,
                City = request.City,
                Country = request.Country,
                Address = request.Address,
                IdentifierNumber = request.Identify,
                Status = 0,
                FailedAttempts = 0,
                LockedUntil = null,
                TokenExpiry = null,
                RefreshToken = null,
                CreateOtp = null,
                CreatedAt = DateTime.UtcNow
            };

            var createdAccount = await _accountRepository.CreateFarmer(newAccount);
            if (createdAccount != null)
            {
                int farmerCount = await _accountRepository.CountAccountsByRole("68007b0387b41211f0af1d56");
                int expertCount = await _accountRepository.CountAccountsByRole("68007b2a87b41211f0af1d57");

                await _hubContext.Clients.All.SendAsync("UpdateAccountCount", new
                {
                    farmerCount,
                    expertCount
                });

                await NotifyAccountChangeAsync();



                var usersByProvince = await _statisticRepository.GetUsersByProvinceAsync();
                await _hubContext.Clients.All.SendAsync("UsersByProvince", usersByProvince);

                await _hubContext.Clients.All.SendAsync("UserRegistered", new
                {
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd")
                });
                await _hubContext.Clients.All.SendAsync("UsersByProvinceChanged");

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
        public async Task NotifyAccountChangeAsync()
        {
            var roleIds = new List<string> { "68007b0387b41211f0af1d56", "68007b2a87b41211f0af1d57" };
            var result = await _accountRepository.GetTotalAndGrowthByRoleIdsAsync(roleIds);

            if (result.TryGetValue("FARMER", out var farmer))
            {
                await _hubContext.Clients.All.SendAsync("UpdateFarmerCount", farmer.Count, farmer.Growth);
            }
            if (result.TryGetValue("EXPERT", out var expert))
            {
                await _hubContext.Clients.All.SendAsync("ExpertCountUpdate", expert.Count, expert.Growth);
            }
        }

        //kiểm tra email xem có đúng không , định dạng sai : uyen@gmail,  @gmail.com
        //phải có đủ :
        //^[\w\.\-]+: Bắt đầu với một hoặc nhiều ký tự chữ, số, dấu chấm hoặc dấu gạch ngang.
        //@: Sau đó là ký tự @ bắt buộc.
        //([\w\-]+\.)+: tiếp theo là một hoặc nhiều nhóm tên miền con(như gmail., yahoo.,.gmail.com.vn)
        //[a-zA-Z]{2,}$: cuối cùng kết thúc bằng tên miền chính(vd: com, vn, org) có ít nhất 2 ký tự chữ cái.
        public bool CheckValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var regex = new Regex(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,}$");  //uyen@gmail.com // có thể có nhiều tên miền liền .gmail.com.vn
            return regex.IsMatch(email);
        }

        // kiểm tra sđt phải :
        // bắt đầu bằng 0 hoặc +84
        // theo sau là 1 số  từ 3 đén 9
        // sau đó là đủ 8 số ngẫu nhiên từ 0 đến 9
        public bool CheckValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var regex = new Regex(@"^(0|\+84)[3-9][0-9]{8}$");
            return regex.IsMatch(phone);
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
                    Avatar = "https://firebasestorage.googleapis.com/v0/b/prn221-69738.appspot.com/o/image%2Fdefault-avatar.png?alt=media&token=3857a1ec-ace4-4329-8f36-4f3a40c3fc67",
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
                    Message = "Account is locked login.",
                    LockedUntil = account.LockedUntil
                };
            }

            return await GenerateToken(account);

        }

        public async Task<LoginResponseDTO?> Logout(string? username)
        {
            if (username == null)
                return null;

            await _accountRepository.DeleteRefreshToken(username);

            return new LoginResponseDTO
            {
                AccessToken = null,
                Message = "Logout successfully.",
                RefreshToken = null
            };
        }
    }
}
