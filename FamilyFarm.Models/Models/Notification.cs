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
        public required ObjectId NotifiId { get; set; }
        [BsonRequired]
        public required ObjectId CategoryNotifiId { get; set; }
        [BsonRequired]
        public required ObjectId AccId { get; set; }
        public ObjectId? PostId { get; set; }
        public ObjectId? ServiceId { get; set; }
        public ObjectId? GroupId { get; set; }
        public ObjectId? ProcessId { get; set; }
        public required DateTime CreateAt { get; set; }
    }
}
