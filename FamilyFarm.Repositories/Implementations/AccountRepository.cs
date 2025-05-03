using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Bson;

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


    }
}
