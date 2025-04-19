using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class PostTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId PostTagId { get; set; }
        public DateTime CreatedAt { get; set; }
        public required ObjectId PostId { get; set; }
        public required ObjectId AccId { get; set; }
    }
}
