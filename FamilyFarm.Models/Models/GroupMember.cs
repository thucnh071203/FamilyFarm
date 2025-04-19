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
        public required ObjectId GroupMemberId { get; set; }
        [BsonRequired] 
        public required ObjectId GroupRoleId { get; set; }
        [BsonRequired]
        public required ObjectId GroupId { get; set; }
        [BsonRequired]
        public required ObjectId AccId { get; set; }
        public required DateTime JointAt { get; set; }
        public required string MemberStatus { get; set; }
        public ObjectId? InviteByAccId { get; set; }
        public required DateTime LeftAt { get; set; }
    }
}
