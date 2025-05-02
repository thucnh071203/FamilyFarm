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
        public string HashTagId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }
        public string? HashTagContent { get; set; }
        public DateTime? CreateAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
