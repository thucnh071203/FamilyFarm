using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDAO _dao;
        public AccountRepository(AccountDAO dao)
        {
            _dao = dao;
        }

        public async Task<Account> GetAccountByEmail(string email)
        {
            return await _dao.GetByIdAsync(null, null, email, null);
        }

        public Task<Account> GetAccountById(string acc_id)
        {
            return _dao.GetByIdAsync(acc_id, null, null, null);
        }

        public async Task<Account?> GetAccountByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            // Kiểm tra theo Email
            Account account = await _dao.GetByIdAsync(null, null, identifier, null);
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

        public Task<Account> GetAccountByPhone(string phone)
        {
            return _dao.GetByIdAsync(null, null, null, phone);
        }

        public Task<Account> GetAccountByUsername(string username)
        {
            return _dao.GetByIdAsync(null, username, null, null);
        }

        public Task<List<Account>> GetAll(string role_id, int status)
        {
            return _dao.GetAllAsync(role_id, status);
        }
    }
}
