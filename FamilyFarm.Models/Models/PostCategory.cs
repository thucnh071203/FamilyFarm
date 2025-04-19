using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    class PostCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId CategoryPostId { get; set; }
        public required ObjectId CategoryId { get; set; }
        public required ObjectId PostId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
