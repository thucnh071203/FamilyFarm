using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;

namespace FamilyFarm.Repositories
{
    public interface IAccountRepository
    {
        //Get all account
        //role_id: 1 is ADMIN, 2 is FARMER, 3 is EXPERT
        //status: 0 is ACTIVED, 1 is DELETED, 2 is LOCK, 3 if get all
        Task<List<Account>> GetAll(string role_id, int status);

        //Get by Account Id (not check status)
        Task<Account?> GetAccountById(string acc_id);
        
        // Create Account
        Task<Account> CreateAsync(Account account);
        
        // Update Account (check status)
        Task<Account> UpdateAsync(string id, Account account);
        
        // Delete Account (Set Status = 1)
        Task DeleteAsync(string id);
        
        //Get by Username (not check status)
        Task<Account?> GetAccountByUsername(string username);

        //Get by Email
        Task<Account?> GetAccountByEmail(string email);

        //Get by Phone
        Task<Account?> GetAccountByPhone(string phone);

        //Get by Identifier: username, email, and phone
        Task<Account?> GetAccountByIdentifier(string identifier);

        //Update Refresh token
        Task<bool> UpdateRefreshToken(string? acc_id, string? refreshToken, DateTime? expiry);

        //Get Account by Refresh token
        Task<Account?> GetAccountByRefreshToken(string refreshTokens);

        //Update fail login attempt
        Task<bool> UpdateLoginFail(string? acc_id, int? failAttempts, DateTime? lockedUntil);

        Task<Account?> GetByFacebookId(string facebookId);
        Task<Account> CreateFacebookAccount(string fbId, string name, string email, string avatar);
    }
}
