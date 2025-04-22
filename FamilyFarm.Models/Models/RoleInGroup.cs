using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class RoleInGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string GroupRoleId { get; set; }
        public required string GroupRoleName { get; set; }
        public required string GroupRoleDescription { get; set; }
    }
}
