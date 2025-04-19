using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class ReviewService
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId ReviewId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public required ObjectId AccId { get; set; }
        public required ObjectId BookingServiceId { get; set; }
    }
}
