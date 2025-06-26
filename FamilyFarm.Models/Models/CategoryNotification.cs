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
        public required string CategoryNotifiId { get; set; }
        public required string CategoryNotifiName { get; set; }  // thông báo là hành động gì (ví dụ: "Like", "Comment", "Share", "Joined Group", ...)
        public required string Description { get; set; }
    }
}
