using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.PasswordHashing;
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
    }
}
