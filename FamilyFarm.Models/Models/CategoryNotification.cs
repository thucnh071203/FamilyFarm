using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class CategoryNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId CategoryNotiId { get; set; }
        public required string CategoryNotiName { get; set; }
        public required string Description { get; set; }
    }
}
