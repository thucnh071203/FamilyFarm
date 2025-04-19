using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId PostId { get; set; }
        public required string PostContent { get; set; }
        public required string PostScope { get; set; } = "Public";
        public bool? IsInGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public required int AccId { get; set; }
        public int? GroupId { get; set; }
    }
}
