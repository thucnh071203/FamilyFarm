using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class PaymentTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string PaymentId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BookingServiceId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SubProcessId { get; set; }
        public string? FromAccId { get; set; }
        public string? ToAccId { get; set; }
        public bool? IsRepayment { get; set; }
        public DateTime? PayAt { get; set; }
    }
}
