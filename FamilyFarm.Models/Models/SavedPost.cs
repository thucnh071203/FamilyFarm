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
        public required ObjectId SavedPostId { get; set; }
        [BsonRequired]
        public required ObjectId AccId { get; set; }
        [BsonRequired]
        public required ObjectId PostId { get; set; }
        public DateTime? SavedAt { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
