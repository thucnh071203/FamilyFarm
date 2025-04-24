using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string NotifiId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryNotifiId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ServiceId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? GroupId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProcessId { get; set; }
        public required DateTime CreateAt { get; set; }
    }
}
