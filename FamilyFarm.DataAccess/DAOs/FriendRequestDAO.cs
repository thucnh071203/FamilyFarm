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
        private readonly IMongoCollection<Friend> _Friends;
        public FriendRequestDAO(IMongoDatabase database)
        {
            _Friends = database.GetCollection<Friend>("Friend");
        }


        /// <summary>
        /// Retrieves a list of pending friend requests sent by the specified sender ID.
        /// </summary>
        /// <param name="senderId">The ID of the sender who initiated the friend requests.</param>
        /// <returns>
        /// A list of friend requests sent by the specified sender with a status of "Pending". 
        /// If the sender ID is invalid (cannot be parsed as an ObjectId), returns null.
        /// </returns>
        public async Task<List<Friend?>> GetSentFriendRequestsAsync(string senderId)
        {
            if (!ObjectId.TryParse(senderId, out _))
                return null;
            return await _Friends.Find(r => r.SenderId == senderId && r.Status == "Pending").ToListAsync();
        }



        public async Task<List<Friend?>> GetReceiveFriendRequestsAsync(string receveiId)
        {
            if (!ObjectId.TryParse(receveiId, out _))
                return null;
            return await _Friends.Find(r => r.ReceiverId == receveiId && r.Status == "Pending").ToListAsync();
        }



    }
}
