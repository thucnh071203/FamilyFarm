using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class CategoryReaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryReactionId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRequired]
        public required string ReactionName { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
