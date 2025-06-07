using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class PostTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostTagId { get; set; }
        public DateTime? CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string AccId { get; set; }
        public string? Username { get; set; }
        public string? Fullname { get; set; }
        public string? Avatar {  get; set; }
    }
}
