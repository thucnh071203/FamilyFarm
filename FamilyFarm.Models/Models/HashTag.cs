using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class HashTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId HashTagId { get; set; }
        [BsonRequired]
        public required ObjectId PostId { get; set; }
        public required string HashTagContent { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
