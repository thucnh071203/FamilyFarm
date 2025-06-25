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
        public string? StepId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProcessId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SubprocessId { get; set; }
        public int StepNumber { get; set; }
        public string? StepTitle { get; set; }
        public string? StepDesciption { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
