using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class GroupMember
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string GroupMemberId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string GroupRoleId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string GroupId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        public required DateTime JointAt { get; set; }
        public required string MemberStatus { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? InviteByAccId { get; set; }
        public required DateTime? LeftAt { get; set; }
    }
}
