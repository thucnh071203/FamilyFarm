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
        public string CategoryReactionId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AccId { get; set; }
        [BsonRequired]
        public string ReactionName { get; set; }
        public DateTime? LastModified { get; set; }
        public string IconUrl { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
