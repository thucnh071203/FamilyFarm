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
        public string NotifiId { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryNotifiId { get; set; } // thông báo là hành động gì (ví dụ: "Like", "Comment", "Share", "Joined Group", ...)
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SenderId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? TargetId { get; set; }
        public string? TargetType { get; set; } // đối tượng mà hành động đó tác động lên (ví dụ: "Post", "Service", "Group", "Process")
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
