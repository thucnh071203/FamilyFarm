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
        public required ObjectId SharePostTagId { get; set; }
        public required ObjectId SharePostId { get; set; }
        public required ObjectId AccId { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
