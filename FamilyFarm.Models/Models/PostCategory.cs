using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class PostCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryPostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string PostId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
