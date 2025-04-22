using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    class SharePostTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SharePostTagId { get; set; }
        public required string SharePostId { get; set; }
        public required string AccId { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
