using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel;

namespace FamilyFarm.Models.Models
{
    public class ChatDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ChatDetailId { get; set; }
        public string? Message { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public DateTime SendAt { get; set; } = DateTime.UtcNow;
        public bool? IsRecalled{ get; set; } = false;
        public bool IsSeen { get; set; } = false;
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ChatId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SenderId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ReceiverId { get; set; }
    }
}
