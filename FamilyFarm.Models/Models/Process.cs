using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Process
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ProcessId { get; set; }
        [BsonRequired]
        public required string ExpertId { get; set; }
        [BsonRequired]
        public required string FarmerId { get; set; }
        public required string ProcessTittle { get; set; }
        public required string Description { get; set; }
        public required int NumberOfSteps { get; set; }
        public required int ContinueStep { get; set; }
        public required string ProcessStatus { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeleteAt { get; set; }
        public bool? IsCompletedByExpert { get; set; }
        public bool? IsCompletedByFarmer { get; set; }
        public bool? IsDelete { get; set; }

    }
}
