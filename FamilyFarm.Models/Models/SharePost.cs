using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class SharePost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SharePostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string PostId { get; set; }
        public required string SharePostContent { get; set; }
        public DateTime SharePostAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
