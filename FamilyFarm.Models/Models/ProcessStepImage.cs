using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class ProcessStepImage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProcessStepImageId {  get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProcessStepId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
