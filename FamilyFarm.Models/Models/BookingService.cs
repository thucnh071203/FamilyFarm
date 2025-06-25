using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class BookingService
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string BookingServiceId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ServiceId { get; set; }
        public decimal? Price { get; set; }
        public decimal? CommissionRate { get; set; }
        public DateTime? BookingServiceAt { get; set; }
        public string? BookingServiceStatus { get; set; }
        public DateTime? CancelServiceAt { get; set; }
        public DateTime? RejectServiceAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsPaidByFarmer { get; set; }
        public bool? IsPaidToExpert { get; set; }

    }
}
