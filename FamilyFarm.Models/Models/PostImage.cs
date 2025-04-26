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
        public string PostImageId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }
        [BsonRequired]
        public string ImageUrl { get; set; }
        public bool? IsDeleted { get; set; } = false;
    }
}
