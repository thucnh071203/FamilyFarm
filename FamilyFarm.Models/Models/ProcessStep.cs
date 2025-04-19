using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class ProcessStep
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId StepId { get; set; }
        [BsonRequired]
        public required ObjectId ProcessId { get; set; }
        [BsonRequired]
        public required int StepNumber { get; set; }
        [BsonRequired]
        public required string StepTitle { get; set; }
        public required string StepDesciption { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
