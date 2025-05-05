using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Reaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ReactionId { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string EntityId { get; set; } // PostId hoặc CommentId
        [BsonRequired]
        public required string EntityType { get; set; } // "Post" hoặc "Comment"
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryReactionId { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
    }
}
