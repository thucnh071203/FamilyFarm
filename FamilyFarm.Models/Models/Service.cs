using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Service
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ServiceId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryServiceId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ProviderId { get; set; }
        public required string ServiceName { get; set; }
        public required string ServiceDescription { get; set; }
        public required decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; }
        public decimal? AverageRate { get; set; }
        public int? RateCount { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? HaveProcess { get; set; }
    }
}
