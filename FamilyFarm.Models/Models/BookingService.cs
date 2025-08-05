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
        public string? BookingServiceId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ExpertId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? CommissionRate { get; set; }
        public DateTime? BookingServiceAt { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string? BookingServiceStatus { get; set; }
        public bool? IsCompletedFinal { get; set; }
        public DateTime? CompleteServiceAt { get; set; }
        public DateTime? CancelServiceAt { get; set; }
        public DateTime? RejectServiceAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsPaidByFarmer { get; set; }
        public bool? IsPaidToExpert { get; set; }

        //EXTRA PROCESS
        public bool HasExtraProcess { get; set; }
        public string? ExtraDescription { get; set; }
    }
}
