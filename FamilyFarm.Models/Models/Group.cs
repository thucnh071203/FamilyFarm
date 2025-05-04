using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string GroupId { get; set; }
        public required string GroupName { get; set; }
        public required string? GroupAvatar { get; set; }
        public required string? GroupBackground { get; set; }
        public required string PrivacyType { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string OwnerId { get; set; }
        public required DateTime? CreatedAt { get; set; }
        public required DateTime? UpdatedAt { get; set; }
        public required DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
