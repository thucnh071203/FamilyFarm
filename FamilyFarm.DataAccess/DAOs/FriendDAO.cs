using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class FriendDAO : SingletonBase
    {
        private readonly IMongoCollection<Friend> _Friend;
        private readonly IMongoCollection<Account> _Account;
        private readonly AccountDAO _accountDao;

        public FriendDAO(IMongoDatabase database, AccountDAO accountDao)
        {
            _Friend = database.GetCollection<Friend>("Friend");
            _Account = database.GetCollection<Account>("Account");
            _accountDao = accountDao;
        }

        public async Task<List<Friend>> GetAllAsync()
        {
            return await _Friend.Find(_ => true).ToListAsync();
        }
        /// <summary>
        /// use to get list friend of user, list friend of others
        /// </summary>
        /// <param name="userid"> cũng có thể là receiverId</param>
        /// <returns></returns>
        public async Task<List<Account>> GetListFriends(string userId, string roleId)
        {

            // Lấy danh sách receiverId từ các Friend đã là friend
            var friendFilter = Builders<Friend>.Filter.And(
        Builders<Friend>.Filter.Eq(f => f.Status, "Friend"),
        Builders<Friend>.Filter.Or(
            Builders<Friend>.Filter.Eq(f => f.SenderId, userId),
            Builders<Friend>.Filter.Eq(f => f.ReceiverId, userId)
            )
            );


            var friends = await _Friend.Find(friendFilter).ToListAsync();

            var friendIds = friends.Select(f =>
            f.SenderId == userId ? f.ReceiverId : f.SenderId
            ).Distinct().ToList();

            //  Lấy thông tin account từ receiverIds
            var accountFilter = Builders<Account>.Filter.In(a => a.AccId, friendIds) & Builders<Account>.Filter.Eq(b => b.RoleId, roleId);
            var friendAccounts = await _Account.Find(accountFilter).ToListAsync();


            return friendAccounts;
        }

        /// <summary>
        /// use to get list follower of user
        /// </summary>
        /// <param name="receiverId">là người nhận được yêu cầu kết bạn, tức là người đang có follower</param>
        /// <returns></returns>
        public async Task<List<Account>> GetListFollower(string receiverId)
        {

            var friendFilter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.ReceiverId, receiverId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Following")
            );

            var friends = await _Friend.Find(friendFilter).ToListAsync();
            var senderId = friends.Select(f => f.SenderId).ToList();


            var accountFilter = Builders<Account>.Filter.In(a => a.AccId, senderId);
            var followerAccounts = await _Account.Find(accountFilter).ToListAsync();

            return followerAccounts;
        }

        /// <summary>
        /// use to get list following of senderId, tức là user
        /// </summary>
        /// <param name="senderId">là người gửi yêu cầu kết bạn đến receiverId, tức đang following receiverId </param>
        /// <returns></returns>
        public async Task<List<Account>> GetListFollowing(string senderId, string roleId)
        {

            var friendFilter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.SenderId, senderId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Following")
            );

            var friends = await _Friend.Find(friendFilter).ToListAsync();
            var receiverId = friends.Select(f => f.ReceiverId).ToList();


            var accountFilter = Builders<Account>.Filter.In(a => a.AccId, receiverId) & Builders<Account>.Filter.Eq(b => b.RoleId, roleId);
            var followerAccounts = await _Account.Find(accountFilter).ToListAsync();

            return followerAccounts;
        }

        public async Task<Friend?> GetByIdAsync(string? Friend_id)
        {
            if (!string.IsNullOrEmpty(Friend_id))
            {
                return await _Friend.Find(r => r.FriendId == Friend_id).FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<string> CheckIsFriendAsync(string senderId, string receiverId)
        {
            var filter = Builders<Friend>.Filter.Or(
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, senderId),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, receiverId)
                ),
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, receiverId),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, senderId)
                )
            );

            var friend = await _Friend.Find(filter).FirstOrDefaultAsync();
            return friend?.Status ?? "null";
        }

        public async Task CreateAsync(Friend Friend)
        {
            await _Friend.InsertOneAsync(Friend);
        }

        public async Task UpdateAsync(string Friend_id, Friend Friend)
        {
            if (string.IsNullOrEmpty(Friend_id))
            {
                await _Friend.ReplaceOneAsync(p => p.FriendId == Friend_id, Friend);
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(Friend_id));
            }
        }

        public async Task DeleteAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                await _Friend.DeleteOneAsync(p => p.FriendId == id);
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(id));
            }
        }

        public async Task<bool> Unfriend(string senderId, string receiverId)
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
            {
                throw new ArgumentException("Sender and ReceiverId are null or empty.");
            }

            var filter = Builders<Friend>.Filter.Or(
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, senderId),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, receiverId)
                ),
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, receiverId),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, senderId)
                )
            );

            var result = await _Friend.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        //get friend suggestion for farmer hoặc expert
        public async Task<List<Account>> GetListSuggestionFriends(string userId, int number)
        {
            // 1. Lấy account hiện tại
            var currentUser = await _Account.Find(a => a.AccId == userId).FirstOrDefaultAsync();
            if (currentUser == null) return new List<Account>();

            var currentRole = currentUser.RoleId;

            // 2. Lấy danh sách ID đã có mối quan hệ với currentUser
            var relatedIds = await _Friend.Find(f =>
                f.SenderId == userId || f.ReceiverId == userId
            ).ToListAsync();

            // Nếu bảng Friend chưa có dữ liệu thì relatedUserIds sẽ rỗng
            var relatedUserIds = relatedIds != null && relatedIds.Any()
                ? relatedIds
                    .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                    .Distinct()
                    .ToList()
                : new List<string>();

            relatedUserIds.Add(userId); // Không gợi ý chính mình

            var suggestions = await _Account.Find(a =>
           a.RoleId == currentRole &&
           !relatedUserIds.Contains(a.AccId))
       .Limit(number * 2) // lấy dư ra để sau khi sort còn đủ kết quả
       .ToListAsync();

            // 4. Sắp xếp: account có cùng City với currentUser sẽ nằm đầu danh sách
            var sortedSuggestions = suggestions
                .OrderByDescending(a => a.City == currentUser.City) // true = 1, false = 0 => true lên đầu
                .Take(number) // Giới hạn lại số lượng yêu cầu
                .ToList();

            return sortedSuggestions;
        }

        public async Task<List<Account>> GetSuggestedExperts(string userId, int number)
        {
            // 1. Lấy tài khoản hiện tại
            var currentUser = await _Account.Find(a => a.AccId == userId).FirstOrDefaultAsync();
            if (currentUser == null) return new List<Account>();

            // 2. Lấy RoleId của Expert 
            var expertRoleId = "68007b2a87b41211f0af1d57"; // ID role của Expert 

            // 3. Lấy danh sách các expert mà người dùng đã follow (status = "Following")
            var followedExperts = await _Friend
                .Find(f => f.SenderId == userId && f.Status == "Following")
                .ToListAsync() ?? new List<Friend>(); // đảm bảo không null

            var followedExpertIds = followedExperts
                .Select(f => f.ReceiverId)
                .Distinct()
                .ToList();

            followedExpertIds.Add(userId); // loại trừ chính mình

            // 4. Gợi ý expert chưa được follow
            var suggestions = await _Account.Find(a =>
                    a.RoleId == expertRoleId &&
                    !followedExpertIds.Contains(a.AccId))
                .Limit(number)
                .ToListAsync();

            return suggestions ?? new List<Account>();
        }


        public async Task<(List<Account> Farmers, List<Account> Experts)> GetAvailableFarmersAndExpertsAsync(string accId)
        {
            // 1. Tìm tất cả liên kết của accId trong bảng Friend (gửi hoặc nhận)
            var relatedFriendships = await _Friend.Find(f =>
                f.SenderId == accId || f.ReceiverId == accId
            ).ToListAsync();

            // 2. Tập hợp tất cả người đã có mối quan hệ
            var relatedAccountIds = relatedFriendships
                .Select(f => f.SenderId == accId ? f.ReceiverId : f.SenderId)
                .ToHashSet();

            relatedAccountIds.Add(accId); // Đừng trả về chính accId

            // 3. Truy vấn tất cả accounts có Role là farmer hoặc expert và chưa liên quan
            var availableAccounts = await _Account.Find(a =>
                (a.RoleId == "68007b0387b41211f0af1d56" || a.RoleId == "68007b2a87b41211f0af1d57") &&
                !relatedAccountIds.Contains(a.AccId)
            ).ToListAsync();

            // 4. Phân loại ra farmer và expert
            var farmers = availableAccounts.Where(a => a.RoleId == "68007b0387b41211f0af1d56").ToList();
            var experts = availableAccounts.Where(a => a.RoleId == "68007b2a87b41211f0af1d57").ToList();

            return (farmers, experts);
        }

        public async Task<List<Account>> SearchUsers(string userId, string keyword, int number)
        {
            // 1. Lấy account hiện tại
            var currentUser = await _Account.Find(a => a.AccId == userId).FirstOrDefaultAsync();
            if (currentUser == null) return new List<Account>();

            var currentRole = currentUser.RoleId;

            // 2. Lấy danh sách bạn bè (nếu cần hiển thị mutual friend)
            var relatedIds = await _Friend.Find(f =>
                f.SenderId == userId || f.ReceiverId == userId
            ).ToListAsync();

            var relatedUserIds = relatedIds?
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .Distinct()
                .ToList() ?? new List<string>();

            relatedUserIds.Add(userId); // Không gợi ý chính mình

            // 3. Tìm kiếm user dựa trên keyword (case-insensitive)
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(a => a.RoleId, currentRole) &
                         filterBuilder.Ne(a => a.AccId, userId) & // loại bản thân
                         (filterBuilder.Regex(a => a.Username, new MongoDB.Bson.BsonRegularExpression(keyword, "i")) |
                          filterBuilder.Regex(a => a.FullName, new MongoDB.Bson.BsonRegularExpression(keyword, "i")));

            var suggestions = await _Account.Find(filter)
                .Limit(number * 2) // Lấy dư để sort
                .ToListAsync();

            // 4. Sắp xếp: ưu tiên user cùng City
            var sortedSuggestions = suggestions
                .OrderByDescending(a => a.City == currentUser.City)
                .Take(number)
                .ToList();

            return sortedSuggestions;
        }
    }
}
