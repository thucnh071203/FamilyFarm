using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class SharePostTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SharePostTagId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SharePostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
