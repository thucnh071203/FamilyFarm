using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class SharePost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId SharePostId { get; set; }
        public required string SharePostContent { get; set; }
        public DateTime SharePostAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public required ObjectId AccId { get; set; }
        public required ObjectId PostId { get; set; }
    }
}
