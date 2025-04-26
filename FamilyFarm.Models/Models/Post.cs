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
        public string PostId { get; set; }
        public string? PostContent { get; set; }
        public string? PostScope { get; set; }
        [BsonRepresentation(BsonType.ObjectId)] 
        public string AccId { get; set; }
        public string? GroupId { get; set; }
        public bool IsInGroup { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
