using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class PostImage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId PostImageId { get; set; }
        [BsonRequired]
        public required ObjectId PostId { get; set; }
        [BsonRequired]
        public required string ImageUrl { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
