using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FamilyFarm.DataAccess.DAOs
{
    public class FriendRequestDAO
    {
        private readonly IMongoCollection<Friend> _Friends;
        private readonly IMongoCollection<Account> _Accounts;
        public FriendRequestDAO(IMongoDatabase database)
        {
            _Friends = database.GetCollection<Friend>("Friend");

            _Accounts = database.GetCollection<Account>("Account");
        }



        public async Task<List<Account>> GetSentFriendRequestsAsync(string senderId)
        {

            var friendFilter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.SenderId, senderId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Pending")
            );

            var friends = await _Friends.Find(friendFilter).ToListAsync();
            var receiverId = friends.Select(f => f.ReceiverId).ToList();


            var accFilter = Builders<Account>.Filter.In(a => a.AccId, receiverId);
            var sender = await _Accounts.Find(accFilter).ToListAsync();

            return sender;
        }

        public async Task<List<Account>> GetReceiveFriendRequestsAsync(string receiverId)
        {

            var friendFilter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.ReceiverId, receiverId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Pending")
            );

            var friends = await _Friends.Find(friendFilter).ToListAsync();
            var senderId = friends.Select(f => f.SenderId).ToList();


            var accFilter = Builders<Account>.Filter.In(a => a.AccId, senderId);
            var receiver = await _Accounts.Find(accFilter).ToListAsync();

            return receiver;
        }



        //// Chấp nhận yêu cầu kết bạn 
        //public async Task<bool> AcceptFriendRequestAsync(string friendId)
        //{
        //    var filter = Builders<Friend>.Filter.Eq(f => f.FriendId, friendId);
        //    var update = Builders<Friend>.Update
        //        .Set(f => f.Status, "Friend")
        //        .Set(f => f.UpdateAt, DateTime.UtcNow);

        //    var result = await _Friends.UpdateOneAsync(filter, update);
        //    return result.ModifiedCount > 0;
        //}

        // Chấp nhận yêu cầu kết bạn 
        public async Task<bool> AcceptFriendRequestAsync(string senderId, string reveiverId)
        {
            var filter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.SenderId, reveiverId),
                Builders<Friend>.Filter.Eq(f => f.ReceiverId, senderId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Pending")
            );


            var update = Builders<Friend>.Update
                .Set(f => f.Status, "Friend")
                .Set(f => f.UpdateAt, DateTime.UtcNow);

            var result = await _Friends.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        //// Xóa yêu cầu kết bạn 
        //public async Task<bool> RejectFriendRequestAsync(string senderId)
        //{
        //    var filter = Builders<Friend>.Filter.Eq(f => f.FriendId, friendId);

        //    var result = await _Friends.DeleteOneAsync(filter);
        //    return result.DeletedCount > 0;
        //}

        // Xóa yêu cầu kết bạn 
        public async Task<bool> RejectFriendRequestAsync(string senderId, string receiverId)
        {
            var filter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.SenderId, receiverId),
                Builders<Friend>.Filter.Eq(f => f.ReceiverId, senderId),
                Builders<Friend>.Filter.Eq(f => f.Status, "Pending")
            );

            var result = await _Friends.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }


        // Lấy yêu cầu kết bạn theo FriendId
        public async Task<Friend> GetFriendRequestAsync(string friendId)
        {
            return await _Friends.Find(f => f.FriendId == friendId).FirstOrDefaultAsync();
        }

        // kiểm tra nếu đã có yêu cầu kết bạn "pending"
        public async Task<Friend?> GetPendingRequestAsync(string senderId, string receiverId)
        {
            return await _Friends.Find(f =>
                (f.SenderId == senderId && f.ReceiverId == receiverId && f.Status == "Pending") ||
                (f.SenderId == receiverId && f.ReceiverId == senderId && f.Status == "Pending"))
                .FirstOrDefaultAsync();
        }

        // tạo lời mời kết bạn mới
        public async Task<bool> CreateFriendRequestAsync(string senderId, string receiverId)
        {

            var sender = await _Accounts.Find(a => a.AccId == senderId).FirstOrDefaultAsync();
            var receiver = await _Accounts.Find(a => a.AccId == receiverId).FirstOrDefaultAsync();

            if (receiver == null)
            {
                return false;
            }

            var existingRequest = await GetPendingRequestAsync(senderId, receiverId);

            if (existingRequest != null)
            {
                return false; // Nếu đã có yêu cầu kết bạn "pending"
            }

            if (sender.RoleId == receiver.RoleId)
            {
                // Tạo yêu cầu kết bạn mới
                var friendRequest = new Friend
                {
                    FriendId = ObjectId.GenerateNewId().ToString(),
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    UpdateAt = DateTime.UtcNow,
                    Status = "Pending" // Trạng thái yêu cầu đang chờ
                };
                await _Friends.InsertOneAsync(friendRequest);
                return true;
            }
            else if (sender.RoleId.Equals("68007b0387b41211f0af1d56") && receiver.RoleId.Equals("68007b2a87b41211f0af1d57"))
            {
                // following
                var friendRequest = new Friend
                {
                    FriendId = ObjectId.GenerateNewId().ToString(),
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    UpdateAt = DateTime.UtcNow,
                    Status = "Following" // Trạng thái follow
                };
                await _Friends.InsertOneAsync(friendRequest);
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
