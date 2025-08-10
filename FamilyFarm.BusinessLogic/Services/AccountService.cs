using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHasher _hasher;
        private readonly IUploadFileService _uploadFileService;
        private readonly IMapper _mapper;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        public AccountService(IAccountRepository accountRepository, PasswordHasher hasher, IUploadFileService uploadFileService, IMapper mapper, IHubContext<TopEngagedPostHub> hubContext)
        {
            _accountRepository = accountRepository;
            _hasher = hasher;
            _uploadFileService = uploadFileService;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        public async Task<Account?> GetAccountById(string acc_id)
        {
            return await _accountRepository.GetAccountById(acc_id);
        }

        public async Task<Account?> GetAccountByUsername(string username)
        {
            return await _accountRepository.GetAccountByUsername(username);
        }

        public async Task<Account> CreateAsync(Account account)
        {
            account.PasswordHash = _hasher.HashPassword(account.PasswordHash);
            return await _accountRepository.CreateAsync(account);
        }

        public async Task<Account> UpdateAsync(string id, Account account)
        {
            account.PasswordHash = _hasher.HashPassword(account.PasswordHash);
            return await _accountRepository.UpdateAsync(id, account);
        }

        public async Task<Account> UpdateOtpAsync(string id, Account account)
        {
            return await _accountRepository.UpdateAsync(id, account);
        }

        public async Task DeleteAsync(string id)
        {
            await _accountRepository.DeleteAsync(id);
        }

        public async Task<UpdateProfileResponseDTO> UpdateProfileAsync(string id, UpdateProfileRequestDTO request)
        {
            if (request == null)
            {
                return new UpdateProfileResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Request is null"
                };
            }

            var account = await _accountRepository.GetAccountById(id);

            if (account == null) {
                return new UpdateProfileResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Account not found"
                };
            }

            // 🚫 Check trùng email (nếu thay đổi)
            if (!string.IsNullOrEmpty(request.Email) && request.Email != account.Email)
            {
                var existedEmailAcc = await _accountRepository.GetAccountByEmail(request.Email);
                if (existedEmailAcc != null && existedEmailAcc.AccId != id)
                {
                    return new UpdateProfileResponseDTO
                    {
                        IsSuccess = false,
                        MessageError = "Email already in use."
                    };
                }
            }

            // 🚫 Check trùng số điện thoại (nếu thay đổi)
            if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != account.PhoneNumber)
            {
                var existedPhoneAcc = await _accountRepository.GetAccountByPhone(request.PhoneNumber);
                if (existedPhoneAcc != null && existedPhoneAcc.AccId != id)
                {
                    return new UpdateProfileResponseDTO
                    {
                        IsSuccess = false,
                        MessageError = "Phone number already in use."
                    };
                }
            }

            string finalBgUrl = account.Background;

            //if (request.Background != null)
            //{
            //    var imageURL = await _uploadFileService.UploadImage(request.Background);
            //    if (!string.IsNullOrEmpty(imageURL?.UrlFile))
            //    {
            //        finalBgUrl = imageURL.UrlFile;
            //    }
            //}

            if (request.Background != null)
            {
                try
                {
                    var imageURL = await _uploadFileService.UploadImage(request.Background);
                    if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                    {
                        finalBgUrl = imageURL.UrlFile;
                    }
                }
                catch (Exception ex)
                {
                    // Logging tùy ý
                    Console.WriteLine($"Upload avatar failed: {ex.Message}");
                    // fallback: dùng avatar cũ
                }
            }

            string finalAvtUrl = account.Avatar;

            //if (request.Avatar != null)
            //{
            //    var imageURL = await _uploadFileService.UploadImage(request.Avatar);
            //    if (!string.IsNullOrEmpty(imageURL?.UrlFile))
            //    {
            //        finalAvtUrl = imageURL.UrlFile;
            //    }
            //}

            if (request.Avatar != null)
            {
                try
                {
                    var imageURL = await _uploadFileService.UploadImage(request.Avatar);
                    if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                    {
                        finalAvtUrl = imageURL.UrlFile;
                    }
                }
                catch (Exception ex)
                {
                    // Logging tùy ý
                    Console.WriteLine($"Upload avatar failed: {ex.Message}");
                    // fallback: dùng avatar cũ
                }
            }

            string finalCertificateUrl = account.Certificate;

            if (request.Certificate != null)
            {
                var imageURL = await _uploadFileService.UploadImage(request.Certificate);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalCertificateUrl = imageURL.UrlFile;
                }
            }

            account.FullName = request.FullName;
            account.Birthday = request.Birthday;
            account.Gender = request.Gender;
            account.PhoneNumber = request.PhoneNumber;
            account.Email = request.Email;
            account.City = request.City;
            account.Country = request.Country;
            account.Address = request.Address;
            account.Avatar = finalAvtUrl;
            account.Background = finalBgUrl;
            account.Certificate = finalCertificateUrl;
            account.WorkAt = request.WorkAt;
            account.StudyAt = request.StudyAt;

            var result = await _accountRepository.UpdateAsync(account.AccId, account);

            if (result == null)
            {
                return new UpdateProfileResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Update fail!"
                };
            }

            return new UpdateProfileResponseDTO
            {
                IsSuccess = true,
                MessageError = null,
                Data = result
            };
        }

        public async Task<MyProfileDTO?> GetUserProfileAsync(string accId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accId);

            if (account == null) return null;

            var result = _mapper.Map<MyProfileDTO>(account);
            return result;
        }

        public async Task<UpdateAvatarResponseDTO?> ChangeOwnAvatar(string? accountId, UpdateAvatarRequesDTO? request)
        {
            if(accountId == null) return null;

            var account = await _accountRepository.GetAccountById(accountId);

            if(account == null) return null;

            var oldAvatar = account.Avatar;

            //Upload lên firebase avatar mới
            if(request == null || request.NewAvatar == null)
            {
                return null;
            }

            var newAvatar = await _uploadFileService.UploadImage(request.NewAvatar);
            if(newAvatar == null || newAvatar.UrlFile == null) 
                return null;

            string? data = await _accountRepository.UpdateAvatar(accountId, newAvatar.UrlFile);

            if(data == null) return null;

            return new UpdateAvatarResponseDTO
            {
                Message = "Update avatar successfully.",
                Success = true,
                Data = data
            };
        }

        public async Task<UpdateBackgroundResponseDTO?> ChangeOwnBackground(string? accountId, UpdateBackgroundRequestDTO? request)
        {
            if (accountId == null) return null;

            var account = await _accountRepository.GetAccountById(accountId);

            if (account == null) return null;

            var oldBackground = account.Background;

            // Upload ảnh nền mới lên Firebase
            if (request == null || request.NewBackground == null)
            {
                return null;
            }

            var newBackground = await _uploadFileService.UploadImage(request.NewBackground);
            if (newBackground == null || newBackground.UrlFile == null)
                return null;

            string? data = await _accountRepository.UpdateBackground(accountId, newBackground.UrlFile);

            if (data == null) return null;

            return new UpdateBackgroundResponseDTO
            {
                Message = "Update background successfully.",
                Success = true,
                Data = data
            };
        }

        //public async Task<TotalFarmerExpertDTO<Dictionary<string, int>>> GetTotalByRoleIdsAsync(List<string> roleIds)
        //{
        //    var counts = await _accountRepository.CountByRoleIdsAsync(roleIds);

        //    return new TotalFarmerExpertDTO<Dictionary<string, int>>
        //    {
        //        IsSuccess = true,
        //        Message = "Counts by RoleId fetched successfully",
        //        Data = counts
        //    };
        //}
        public async Task<Dictionary<string, (int Count, int Growth)>> GetTotalAndGrowthByRoleIdsAsync(List<string> roleIds)
        {
            return await _accountRepository.GetTotalAndGrowthByRoleIdsAsync(roleIds);
        }

        public async Task<TotalFarmerExpertDTO<Dictionary<string, int>>> GetUserGrowthOverTimeAsync(DateTime fromDate, DateTime toDate)
        {
            var result = await _accountRepository.GetUserGrowthOverTimeAsync(fromDate, toDate);

            return new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = $"User growth from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}.",
                Data = result
            };
        }

        public async Task<List<Account>?> GetAllAccountByRoleId(string role_id)
        {
           // if (role_id == null) return null;
            return await _accountRepository.GetAllAccountByRoleId(role_id);
        }
        //public async Task<bool> UpdateAccountStatus(string accId, int status)
        //{
        //    return await _accountRepository.UpdateAccountStatus(accId, status);
        //}

        public async Task<bool> UpdateAccountStatus(string accId, int status)
        {
            var result = await _accountRepository.UpdateAccountStatus(accId, status);

            if (result && status == 1) 
            {
                var expertRoleIds = new List<string> { "68007b2a87b41211f0af1d57" };
                var totalByRoles = await _accountRepository.GetTotalAndGrowthByRoleIdsAsync(expertRoleIds);
                var expertCount = totalByRoles.ContainsKey("68007b2a87b41211f0af1d57")
              ? totalByRoles["68007b2a87b41211f0af1d57"].Count
              : 0;

                await _hubContext.Clients.All.SendAsync("ExpertCountUpdate", expertCount);
            }

            return result;
        }


        public async Task<Account?> GetAccountByAccId(string accId)
        {
            if (accId == null) return null;
            return await _accountRepository.GetAccountByAccId(accId);
        }

        public async Task<List<Account>> GetAllAccountExceptAdmin()
        {
            return await _accountRepository.GetAllAccountExceptAdmin();
         }

        public async Task<ForgotPasswordResponseDTO> GetAccountByEmail(string email)
        {
            if (email == null)
            {
                return new ForgotPasswordResponseDTO
                {
                    Success = false,
                    Message = "Email is null",
                };
            }

            var getAccByEmail = await _accountRepository.GetAccountByEmail(email);

            if (getAccByEmail == null)
            {
                return new ForgotPasswordResponseDTO
                {
                    Success = false,
                    Message = "Account is not found",
                };
            } else if (getAccByEmail.Status == 1)
            {
                return new ForgotPasswordResponseDTO
                {
                    Success = false,
                    Message = "Account is locked",
                };
            }

            return new ForgotPasswordResponseDTO
            {
                Success = true,
                Message = "Get account successful",
                Data = getAccByEmail
            };

        }

        public async Task<Account?> CheckAccountByEmail(string? email)
        {
            return await _accountRepository.GetAccountByEmail(email);
        }

        public async Task<Account?> CheckAccountByPhone(string? phone)
        {
            return await _accountRepository.GetAccountByPhone(phone);
        }

        public async Task<Account?> UpdateCreditCard(string id, CreditCardUpdateRequestDTO request)
        {
            return await _accountRepository.UpdateCreditCard(
                id,
                true,
                request.CreditNumber,
                request.CreditName,
                request.ExpiryDate
            );
        }
    }
}
