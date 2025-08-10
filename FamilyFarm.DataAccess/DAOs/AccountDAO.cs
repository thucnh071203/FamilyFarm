using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class AccountDAO
    {
        private readonly IMongoCollection<Account> _Accounts;
        private readonly IMongoCollection<Role> _Roles;
        public AccountDAO(IMongoDatabase database)
        {
            _Accounts = database.GetCollection<Account>("Account");
            _Roles = database.GetCollection<Role>("Role");
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
            if (string.IsNullOrEmpty(role_id) && role_map.Contains(role_id))
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
                // Nếu acc_id không đúng định dạng ObjectId thì trả về null
                if (!ObjectId.TryParse(acc_id, out _))
                {
                    return null;
                }
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

        /// <summary>
        ///     Creates a new account in the database with a generated unique ID and default status.
        /// </summary>
        /// <param name="account">The account object to be created.</param>
        /// <returns>The created account object including its generated ID.</returns>
        public async Task<Account> CreateAsync(Account account)
        {
            account.AccId = ObjectId.GenerateNewId().ToString();
            account.Status = 0;
            await _Accounts.InsertOneAsync(account);
            return account;
        }

        /// <summary>
        ///     Updates an existing account by its ID if it is not marked as deleted.
        /// </summary>
        /// <param name="id">The ID of the account to update.</param>
        /// <param name="updatedAccount">The new account data to replace the existing one.</param>
        /// <returns>The updated account object, or null if not found or deleted.</returns>
        public async Task<Account> UpdateAsync(string id, Account updatedAccount)
        {
            var existing = await _Accounts.Find(a => a.AccId == id && a.Status == 0).FirstOrDefaultAsync();
            if (existing == null) return null;

            updatedAccount.AccId = id; // Ensure ID doesn't change
            await _Accounts.ReplaceOneAsync(a => a.AccId == id && a.Status == 0, updatedAccount);
            return updatedAccount;
        }

        /// <summary>
        ///     Soft deletes an account by setting its status to 1 (marked as deleted).
        /// </summary>
        /// <param name="id">The ID of the account to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(string id)
        {
            var filter = Builders<Account>.Filter.Where(a => a.AccId == id && a.Status == 0);
            var update = Builders<Account>.Update.Set(a => a.Status, 1); // Status = 1 (Is Deleted)

            await _Accounts.UpdateOneAsync(filter, update);
        }

        /// <summary>
        ///     To get Account with facebookId
        /// </summary>
        public async Task<Account?> GetByFacebookIdAsync(string facebookId)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.FacebookId, facebookId);
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Create a new account with facebook if that account has never been logged in
        /// </summary>
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
                Status = 0,
                RoleId = "68007b0387b41211f0af1d56", // Mặc định là FARMER
                FacebookId = fbId,
                Avatar = avatar,
                IsFacebook = true,
                Otp = -1,
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

        /// <summary>
        ///     Thêm mới một tài khoản Farmer
        /// </summary>
        /// <param name="account">Đối tượng Farmer cần tạo</param>
        /// <returns>Trả về Farmer nếu thành công, null nếu thất bại</returns>
        public async Task<Account?> CreateFarmerAsync(Account account)
        {
            try
            {
                await _Accounts.InsertOneAsync(account);
                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo Farmer: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Use to create new account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task CreateAccount(Account account)
        {
            await _Accounts.InsertOneAsync(account);
        }

        /// <summary>
        /// Use to Get account by identifier number
        /// </summary>
        /// <param name="identifierNumber"> it's cccd/cmnd number</param>
        /// <returns></returns>
        public async Task<Account?> GetAccountByIdentifierNumber(string identifierNumber)
        {
            FilterDefinition<Account> filter;

            if (!string.IsNullOrEmpty(identifierNumber))
            {
                filter = Builders<Account>.Filter.Eq(a => a.IdentifierNumber, identifierNumber);
            }
            else
            {
                return null;
            }
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<Account?> GetAccountByUsernameU(string username)
        {
            FilterDefinition<Account> filter;

            if (!string.IsNullOrEmpty(username))
            {
                filter = Builders<Account>.Filter.Eq(a => a.Username, username);
            }
            else
            {
                return null;
            }
            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }
        /// <summary>
        ///     Delete Refresh Token of user by Username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>False if delete fail, True is success</returns>
        public async Task<bool> DeleteFreshTokenByUsername(string? username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            var filter = Builders<Account>.Filter.Eq(a => a.Username, username);

            var update = Builders<Account>.Update
                .Set(a => a.RefreshToken, null)
                .Set(a => a.TokenExpiry, null);
            var result = await _Accounts.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<Account?> GetAccountByIdAsync(string accId)
        {
            return await _Accounts
                .Find(a => a.AccId == accId && a.Status == 0)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Change avatar
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="avatarUrl">
        ///     Avatar url is new avatar, if no change is old avatar
        /// </param>
        /// <returns>Avatar Url</returns>
        public async Task<string?> UpdateAvatar(string? accountId, string? avatarUrl)
        {
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(avatarUrl)) { return null; }

            var filter = Builders<Account>.Filter.Eq(a => a.AccId, accountId);
            var update = Builders<Account>.Update.Set(a => a.Avatar, avatarUrl);

            var result = await _Accounts.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? avatarUrl : null;
        }

        public async Task<string?> UpdateBackground(string? accountId, string? backgroundUrl)
        {
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(backgroundUrl)) { return null; }

            var filter = Builders<Account>.Filter.Eq(a => a.AccId, accountId);
            var update = Builders<Account>.Update.Set(a => a.Background, backgroundUrl);

            var result = await _Accounts.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? backgroundUrl : null;
        }

        public async Task<List<string>> GetAccountIdsByFullNameAsync(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return new List<string>();

            var filter = Builders<Account>.Filter.Regex(
                a => a.FullName,
                new MongoDB.Bson.BsonRegularExpression(fullName, "i") // Case-insensitive search
            );

            var accounts = await _Accounts.Find(filter).ToListAsync();
            return accounts.Select(a => a.AccId).ToList();
        }

        public async Task<int> CountByRoleIdAsync(string roleId)
        {
            return (int)await _Accounts.CountDocumentsAsync(a => a.RoleId == roleId && a.Status == 1);
        }

        //public async Task<Dictionary<string, int>> GetTotalByRoleIdsAsync(List<string> roleIds)
        //{
        //    var filter = Builders<Account>.Filter.In(a => a.RoleId, roleIds);

        //    var aggregation = await _Accounts.Aggregate()
        //        .Match(filter)
        //        .Group(a => a.RoleId, g => new { RoleId = g.Key, Count = g.Count() })
        //        .ToListAsync();

        //    var roleNames = await _Roles.Find(r => roleIds.Contains(r.RoleId))
        //        .ToListAsync();

        //    return aggregation.ToDictionary(
        //        x => roleNames.FirstOrDefault(r => r.RoleId == x.RoleId)?.RoleName ?? "Unknown",
        //        x => x.Count
        //    );
        //}



        public async Task<Dictionary<string, (int Count, int Growth)>> GetTotalAndGrowthByRoleIdsAsync(List<string> roleIds)
        {
            var filter = Builders<Account>.Filter.In(a => a.RoleId, roleIds);

            var now = DateTime.UtcNow;
            var startOfThisWeek = now.Date.AddDays(-(int)now.DayOfWeek); // Sunday
            var startOfLastWeek = startOfThisWeek.AddDays(-7);
            var endOfLastWeek = startOfThisWeek.AddSeconds(-1);

            var thisWeekFilter = Builders<Account>.Filter.And(
                filter,
                Builders<Account>.Filter.Gte(a => a.CreatedAt, startOfThisWeek),
                Builders<Account>.Filter.Lte(a => a.CreatedAt, now)
            );

            var lastWeekFilter = Builders<Account>.Filter.And(
                filter,
                Builders<Account>.Filter.Gte(a => a.CreatedAt, startOfLastWeek),
                Builders<Account>.Filter.Lte(a => a.CreatedAt, endOfLastWeek)
            );

            var totalAggregation = await _Accounts.Aggregate()
                .Match(filter)
                .Group(a => a.RoleId, g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var thisWeekAggregation = await _Accounts.Aggregate()
                .Match(thisWeekFilter)
                .Group(a => a.RoleId, g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var lastWeekAggregation = await _Accounts.Aggregate()
                .Match(lastWeekFilter)
                .Group(a => a.RoleId, g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var roleNames = await _Roles.Find(r => roleIds.Contains(r.RoleId)).ToListAsync();

            var result = new Dictionary<string, (int Count, int Growth)>();

            foreach (var total in totalAggregation)
            {
                var roleName = roleNames.FirstOrDefault(r => r.RoleId == total.RoleId)?.RoleName ?? "Unknown";

                var thisWeek = thisWeekAggregation.FirstOrDefault(x => x.RoleId == total.RoleId)?.Count ?? 0;
                var lastWeek = lastWeekAggregation.FirstOrDefault(x => x.RoleId == total.RoleId)?.Count ?? 0;

                int growth = (lastWeek == 0 && thisWeek > 0)
                    ? 100
                    : (lastWeek == 0 ? 0 : (int)Math.Round(((double)(thisWeek - lastWeek) / lastWeek) * 100));

                result[roleName.ToUpper()] = (total.Count, growth);
            }

            return result;
        }



        public async Task<Dictionary<string, int>> GetUserGrowthOverTimeAsync(DateTime fromDate, DateTime toDate)
        {
            var groupedData = await _Accounts
                .AsQueryable()
                .Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate)
                .GroupBy(x => new
                {
                    x.CreatedAt.Year,
                    x.CreatedAt.Month,
                    x.CreatedAt.Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = groupedData
                .OrderBy(x => new DateTime(x.Year, x.Month, x.Day))
                .ToDictionary(
                    x => new DateTime(x.Year, x.Month, x.Day).ToString("dd/MM/yyyy"),
                    x => x.Count
                );

            return result;
        }

        /// <summary>
        /// Đếm số lượng tài khoản theo RoleId
        /// </summary>
        /// <param name="roleId">ID của vai trò (Role)</param>
        /// <returns>Số lượng tài khoản có RoleId tương ứng</returns>
        public async Task<int> CountAccountsByRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return 0;
            }

            var filter = Builders<Account>.Filter.Eq(a => a.RoleId, roleId);
            return (int)await _Accounts.CountDocumentsAsync(filter);
        }


        public async Task<List<Account>> GetAllAccountByRoleId(string role_id)
        {
            string[] role_map = {
        "67fd41dfba121b52bbc622c3", // ROLE_ADMIN
        "68007b0387b41211f0af1d56", // ROLE_FARMER
        "68007b2a87b41211f0af1d57"  // ROLE_EXPERT
    };

            var filters = new List<FilterDefinition<Account>>();

            if (!string.IsNullOrEmpty(role_id) && role_map.Contains(role_id))
            {
                filters.Add(Builders<Account>.Filter.Eq(a => a.RoleId, role_id));
            }

            FilterDefinition<Account> finalFilter = filters.Any()
                ? Builders<Account>.Filter.And(filters)
                : Builders<Account>.Filter.Empty;

            return await _Accounts.Find(finalFilter).ToListAsync();
        }


        public async Task<bool> UpdateAccountStatus(string accId, int status)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.AccId, accId);
            var update = Builders<Account>.Update
                        .Set(a => a.Status, status);

            var result = await _Accounts.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        //use for admin
        public async Task<Account?> GetAccountByAccId(string accId)
        {
            return await _Accounts
                .Find(a => a.AccId == accId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Account>> GetAllAccountExceptAdmin()
        {
            const string ROLE_ADMIN_ID = "67fd41dfba121b52bbc622c3";

            var filter = Builders<Account>.Filter.Ne(a => a.RoleId, ROLE_ADMIN_ID);

            return await _Accounts.Find(filter).ToListAsync();
        }

        public async Task<Account?> UpdateCreditCardInfoAsync(string id, bool hasCredit, string? creditNumber, string? creditName, DateTime? expiryDate)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.AccId, id) & Builders<Account>.Filter.Eq(a => a.Status, 0);

            var update = Builders<Account>.Update
                .Set(a => a.HasCreditCard, hasCredit)
                .Set(a => a.CreditNumber, creditNumber)
                .Set(a => a.CreditName, creditName)
                .Set(a => a.ExpiryDate, expiryDate);

            var result = await _Accounts.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0) return null;

            return await _Accounts.Find(filter).FirstOrDefaultAsync();
        }
    }
}
