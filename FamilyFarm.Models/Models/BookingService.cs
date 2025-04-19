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
        public required ObjectId BookingServiceId { get; set; }
        [BsonRequired]
        public required ObjectId AccId { get; set; }
        [BsonRequired]
        public required ObjectId ServiceId { get; set; }
        public required decimal? Price { get; set; }
        public DateTime? BookingServiceAt { get; set; }
        public required string? BookingServiceStatus { get; set; }
        public DateTime? CancelServiceAt { get; set; }
        public required decimal? FirstPayment { get; set; }
        public DateTime? FirstPaymentAt { get; set; }
        public required decimal? SecondPayment { get; set; }
        public DateTime? SecondPaymentAt { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
