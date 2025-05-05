using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class NotificationStatus
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string NotifiStatusId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string NotifiId { get; set; }
        public required bool IsRead { get; set; }
    }
}
