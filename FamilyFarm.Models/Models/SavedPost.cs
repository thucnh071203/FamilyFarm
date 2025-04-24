using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class SavedPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SavedPostId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string PostId { get; set; }
        public DateTime? SavedAt { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
