using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class CategoryService
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CategoryServiceId { get; set; }
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        public required string CategoryName { get; set; }
        public required string CategoryDescription { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
