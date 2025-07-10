using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class Revenue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? RevenueId { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? CommissionRevenue { get; set; }
    }
}
