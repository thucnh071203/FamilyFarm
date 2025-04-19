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
        public required ObjectId ChatId { get; set; }
        [BsonRequired]
        public required ObjectId User1Id { get; set; }
        [BsonRequired]
        public required ObjectId User2Id { get; set; }
        public required DateTime? CreateAt { get; set; }
    }
}
