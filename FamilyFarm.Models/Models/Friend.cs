using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Friend
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId FriendId { get; set; }
        [BsonRequired]
        public required ObjectId SenderId { get; set; }
        [BsonRequired]
        public required ObjectId ReceiverId { get; set; }
        public DateTime? UpdateAt { get; set; }
        public required string Status { get; set; }
    }
}
