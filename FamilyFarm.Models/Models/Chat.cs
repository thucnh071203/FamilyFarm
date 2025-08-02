using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChatId { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Acc1Id { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Acc2Id { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
    }
}
