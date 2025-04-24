using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class ReactionComment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ReactCommentId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CommentId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryReactionId { get; set; }
        public required string? ReactionName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
    }
}
