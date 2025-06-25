using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class StepResultImages
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string StepResultImageId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StepResultId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
