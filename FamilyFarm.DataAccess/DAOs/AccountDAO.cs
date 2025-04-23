using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class AccountDAO
    {
        private readonly IMongoCollection<Account> _Accounts;

        public AccountDAO(IMongoDatabase database)
        {
            _Accounts = database.GetCollection<Account>("Account");    
        }

        /// <summary>
        ///     To get list all account
        /// </summary>
        /// <param name="role_id">role id is a role of account, 1 is ADMIN, 2 is FARMER, 3 is EXPERT</param>
        /// <param name="status">status is status of account: 0 if ACTIVED, 1 if DELETED</param>
        /// <returns>List account with condition</returns>
        public async Task<List<Account>> GetAllAsync(string role_id, int status)
        {
            string[] role_map = {
                "67fd41dfba121b52bbc622c3", //ROLE_ADMIN
                "68007b0387b41211f0af1d56", //ROLE_FARMER
                "68007b2a87b41211f0af1d57" //ROLE_EXPERT
            };

            int[] status_map = { 0, 1, 2 };

            var filters = new List<FilterDefinition<Account>>();

            //Condition 1: Filter status
            if (status > -1 && status_map.Contains(status))
            {
                filters.Add(Builders<Account>.Filter.Eq(a => a.Status, status));
            }
            
            //Condition 2: Filter role_id
            if(string.IsNullOrEmpty(role_id) && role_map.Contains(role_id))
            {
                filters.Add(Builders<Account>.Filter.Eq(a => a.RoleId, role_id));
            }

            var finalFilter = Builders<Account>.Filter.And(filters);

            return await _Accounts.Find(finalFilter).ToListAsync();
        }


        /// <summary>
        ///     To get Account with account Id or Username
        /// </summary>
        /// <param name="acc_id">it is required if getting account with account id</param>
        /// <param name="username">it is required if getting account with username</param>
        /// <returns>Object Account</returns>
        /// <exception cref="ArgumentException">Throw exception when both acc_id and username is NULL</exception>
        public async Task<Account?> GetByIdAsync(string? acc_id, string? username, string? email, string? phone)
        {
            FilterDefinition<Account> filter;

            if (!string.IsNullOrEmpty(acc_id))
            {
                filter = Builders<Account>.Filter.Eq(a => a.AccId, acc_id);
            }
            else if (!string.IsNullOrEmpty(username))
            {
                filter = Builders<Account>.Filter.Eq(a => a.Username, username);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                filter = Builders<Account>.Filter.Eq(a => a.Email, email);
            }
            else if (!string.IsNullOrEmpty(phone))
            {
                filter = Builders<Account>.Filter.Eq(a => a.PhoneNumber, phone);
            }
            else
            {
                return null;
            }
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetByFacebookIdAsync(string facebookId)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.FacebookId, facebookId);
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Account> CreateFacebookAccountAsync(string fbId, string name, string email, string avatar)
        {
            var newAcc = new Account
            {
                AccId = ObjectId.GenerateNewId().ToString(),
                Username = name,
                PasswordHash = "", // Có thể bỏ hoặc để trống vì không dùng đăng nhập truyền thống
                FullName = name,
                Email = email,
                PhoneNumber = "",
                Birthday = null,
                Gender = "Not specified",
                City = "",
                Country = "",
                Status = 1,
                RoleId = "68007b0387b41211f0af1d56", // Mặc định là FARMER
                FacebookId = fbId,
                Avatar = avatar,
                IsFacebook = true,
                CreateOtp = DateTime.UtcNow
            };

            await _Accounts.InsertOneAsync(newAcc);
            return newAcc;
        }


        /// <summary>
        ///     Sử dụng riêng cho Update refresh token và expiry time mới
        /// </summary>
        public async Task<bool> UpdateRefreshToken(string? accId, string? refreshToken, DateTime? expiry)
        {
                var filter = Builders<Account>.Filter.Eq(a => a.AccId, accId);
                var update = Builders<Account>.Update
                    .Set(a => a.RefreshToken, refreshToken)
                    .Set(a => a.TokenExpiry, expiry);
                var result = await _Accounts.UpdateOneAsync(filter, update);
                return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        ///     Sử dụng để lấy Account theo refresh token
        /// </summary>
        public async Task<Account?> GetAccountByRefreshTokenAsync(string refreshToken)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.RefreshToken, refreshToken);
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Sử dụng để update số lần thất bại login và khóa login
        /// </summary>
        public async Task<bool> UpdateLoginFailAsync(string? accId, int? failedAttempts, DateTime? lockedUntil)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.AccId, accId);
            var update = Builders<Account>.Update
                        .Set(a => a.FailedAttempts, failedAttempts)
                        .Set(a => a.LockedUntil, lockedUntil);

            var result = await _Accounts.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

    }
}
