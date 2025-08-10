using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDAO _dao;
        public AccountRepository(AccountDAO dao)
        {
            _dao = dao;
        }

        public async Task<Account?> GetAccountByEmail(string email)
        {
            return await _dao.GetByIdAsync(null, null, email, null);
        }

        public Task<Account?> GetAccountById(string acc_id)
        {
            return _dao.GetByIdAsync(acc_id, null, null, null);
        }

        public async Task<Account?> GetAccountByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            // Kiểm tra theo Email
            Account? account = await _dao.GetByIdAsync(null, null, identifier, null);
            if (account != null)
                return account;

            // Kiểm tra theo Username
            account = await _dao.GetByIdAsync(null, identifier, null, null);
            if (account != null)
                return account;

            // Kiểm tra theo PhoneNumber
            account = await _dao.GetByIdAsync(null, null, null, identifier);
            if (account != null)
                return account;

            return null;
        }

        public Task<Account?> GetAccountByPhone(string phone)
        {
            return _dao.GetByIdAsync(null, null, null, phone);
        }

        public Task<Account?> GetAccountByIdentifierNumber(string identifierNumber)
        {
            return _dao.GetAccountByIdentifierNumber(identifierNumber);
        }

        public Task<Account?> GetAccountByUsernameU(string username)
        {
            return _dao.GetAccountByUsernameU(username);
        }

        public async Task<Account?> GetAccountByRefreshToken(string refreshToken)
        {
            return await _dao.GetAccountByRefreshTokenAsync(refreshToken);
        }

        public Task<Account?> GetAccountByUsername(string username)
        {
            return _dao.GetByIdAsync(null, username, null, null);
        }

        public Task<List<Account>> GetAll(string role_id, int status)
        {
            return _dao.GetAllAsync(role_id, status);
        }

        public async Task<bool> UpdateLoginFail(string? acc_id, int? failAttempts, DateTime? lockedUntil)
        {
            return await _dao.UpdateLoginFailAsync(acc_id, failAttempts, lockedUntil);
        }

        public async Task<bool> UpdateRefreshToken(string? acc_id, string? refreshToken, DateTime? expiry)
        {
            return await _dao.UpdateRefreshToken(acc_id, refreshToken, expiry);
        }


        public async Task CreateAccount(Account account)
        {
            await _dao.CreateAccount(account);
        }


        public async Task<Account?> CreateFarmer(Account newFarmer)
        {
            return await _dao.CreateFarmerAsync(newFarmer);
        }

        public async Task<Account?> GetByFacebookId(string facebookId)
        {
            return await _dao.GetByFacebookIdAsync(facebookId);
        }

        public async Task<Account> CreateFacebookAccount(string fbId, string name, string email, string avatar)
        {
            return await _dao.CreateFacebookAccountAsync(fbId, name, email, avatar);
        }

        public async Task<Account> CreateAsync(Account account)
        {
            return await _dao.CreateAsync(account);
        }

        public async Task<Account> UpdateAsync(string id, Account account)
        {
            return await _dao.UpdateAsync(id, account);
        }

        public async Task DeleteAsync(string id)
        {
            await _dao.DeleteAsync(id);
        }

        public async Task<bool> DeleteRefreshToken(string? username)
        {
            return await _dao.DeleteFreshTokenByUsername(username);
        }
        public async Task<Account?> GetAccountByIdAsync(string accId)
        {
            return await _dao.GetAccountByIdAsync(accId);
        }

        public async Task<string?> UpdateAvatar(string? accountId, string? avatarUrl)
        {
            return await _dao.UpdateAvatar(accountId, avatarUrl);
        }

        public async Task<string?> UpdateBackground(string? accountId, string? backgroundUrl)
        {
            return await _dao.UpdateBackground(accountId, backgroundUrl);
        }

        public async Task<List<string>> GetAccountIdsByFullNameAsync(string fullName)
        {
            return await _dao.GetAccountIdsByFullNameAsync(fullName);
        }
        //public async Task<Dictionary<string, int>> GetTotalByRoleIdsAsync(List<string> roleIds)
        //{
        //    return await _dao.GetTotalByRoleIdsAsync(roleIds);
        //}

        public async Task<Dictionary<string, (int Count, int Growth)>> GetTotalAndGrowthByRoleIdsAsync(List<string> roleIds)
        {
            return await _dao.GetTotalAndGrowthByRoleIdsAsync(roleIds);
        }




        public async Task<Dictionary<string, int>> GetUserGrowthOverTimeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dao.GetUserGrowthOverTimeAsync(fromDate, toDate);
        }

        public async Task<int> CountAccountsByRole(string roleId)
        {
            return await _dao.CountAccountsByRole(roleId);
        }
        public async Task<List<Account>> GetAllAccountByRoleId(string role_id)
        {
            return await _dao.GetAllAccountByRoleId(role_id);
        }
        public async Task<bool> UpdateAccountStatus(string accId, int status)
        {
            return await _dao.UpdateAccountStatus(accId, status);
        }

        public async Task<Account?> GetAccountByAccId(string accId)
        {
            return await _dao.GetAccountByAccId(accId);
        }
        public async Task<List<Account>> GetAllAccountExceptAdmin()
        {
            return await _dao.GetAllAccountExceptAdmin();
        }
        public async Task<Account?> UpdateCreditCard(string id, bool hasCredit, string? creditNumber, string? creditName, DateTime? expiryDate)
        {
            return await _dao.UpdateCreditCardInfoAsync(id, hasCredit, creditNumber, creditName, expiryDate);
        }
    }
}
