using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
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
        public AccountService(IAccountRepository accountRepository, PasswordHasher hasher)
        {
            _accountRepository = accountRepository;
            _hasher = hasher;
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

        public async Task<UpdateProfileResponseDTO> UpdateProfileAsync(string username, UpdateProfileRequestDTO request)
        {
            if (request == null)
            {
                return new UpdateProfileResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Request is null"
                };
            }

            var account = await _accountRepository.GetAccountByUsername(username);

            if (account == null) {
                return new UpdateProfileResponseDTO
                {
                    IsSuccess = false,
                    MessageError = "Account not found"
                };
            }

            account.FullName = request.FullName;
            account.Birthday = request.Birthday;
            account.Gender = request.Gender;
            account.City = request.City;
            account.Country = request.Country;
            account.Address = request.Address;
            account.Background = request.Background;
            account.Certificate = request.Certificate;
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
                MessageError = null
            };
        }

        public async Task<UserProfileResponseDTO?> GetUserProfileAsync(string accId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accId);

            if (account == null) return null;

            return new UserProfileResponseDTO
            {
                AccId = account.AccId,
                Username = account.Username,
                FullName = account.FullName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Avatar = account.Avatar,
                City = account.City,
                Country = account.Country,
                WorkAt = account.WorkAt,
                StudyAt = account.StudyAt
            };
        }
    }
}
