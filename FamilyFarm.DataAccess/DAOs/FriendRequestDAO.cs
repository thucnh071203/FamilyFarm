using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FamilyFarm.DataAccess.DAOs
{
    public class FriendRequestDAO
    {
        private readonly IMongoCollection<Friend> _Requests;
        public FriendRequestDAO(IMongoDatabase database)
        {
            _Requests = database.GetCollection<Friend>("Request");
        }

        public async Task<List<Friend>> GetSentFriendRequestsAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId) || !ObjectId.TryParse(senderId, out ObjectId senderObjId))
                return new List<Friend>();

            var filter = Builders<Friend>.Filter.Eq(f => f.SenderId, senderId) &
                         Builders<Friend>.Filter.Eq(f => f.Status, "Pending");

            return await _Requests.Find(filter).ToListAsync();
        }

        // Phương thức lấy yêu cầu kết bạn đã nhận
        public async Task<List<Friend>> GetReceivedFriendRequestsAsync(string receiverId)
        {
            // Kiểm tra receiverId có hợp lệ không
            if (string.IsNullOrEmpty(receiverId) || !ObjectId.TryParse(receiverId, out ObjectId receiverObjId))
            {
                Debug.WriteLine("Invalid receiverId.");
                return new List<Friend>();  // Trả về danh sách rỗng nếu receiverId không hợp lệ
            }

            // In giá trị receiverObjId
            Debug.WriteLine($"Converted receiverObjId: {receiverObjId}");

            // Xây dựng filter với ObjectId và Status "Pending"
            var filter = Builders<Friend>.Filter.Eq("ReceiverId", receiverObjId) &
                         Builders<Friend>.Filter.Eq("Status", "Pending");

            // In ra filter để kiểm tra
            Debug.WriteLine($"Filter: {filter}");

            // Thực hiện truy vấn
            var result = await _Requests.Find(filter).ToListAsync();

            // Debug thông tin kết quả
            Debug.WriteLine($"Found {result.Count} requests.");

            return result;
        }


    }
}
