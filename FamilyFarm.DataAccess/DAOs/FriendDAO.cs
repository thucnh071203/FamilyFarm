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
        public async Task<List<Account>> GetListSuggestionFriends(string userId)
        {

            // 1. Lấy account hiện tại
            var currentUser = await _Account.Find(a => a.AccId == userId).FirstOrDefaultAsync();
            if (currentUser == null) return new List<Account>();

            var currentRole = currentUser.RoleId;

            // 2. Lấy danh sách ID đã có mối quan hệ với currentUser
            var relatedIds = await _Friend.Find(f =>
                    f.SenderId == userId || f.ReceiverId == userId)
                .ToListAsync();

            var relatedUserIds = relatedIds
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .Distinct()
                .ToList();

            relatedUserIds.Add(userId); // Không gợi ý chính mình

            // 3. Trả về tối đa 8 người cùng role, chưa có quan hệ
            var suggestions = await _Account.Find(a =>
                    a.RoleId == currentRole &&
                    !relatedUserIds.Contains(a.AccId))//chỉ chọn những người mà accId của họ không có trong list relatedUserIds
                .Limit(8)
                .ToListAsync();

            return suggestions;
        }
    }
}
