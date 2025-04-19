using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    class NotificationStatus
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId NotifiStatusId { get; set; }
        public required ObjectId AccId { get; set; }
        public required ObjectId NotifiId { get; set; }
        public required string Status { get; set; }
    }
}
