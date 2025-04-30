using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class SearchHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SearchHistoryId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string AccId { get; set; }
        public required string SearchKey { get; set; }
        public required DateTime SearchedAt { get; set; }
        public required bool IsDeleted { get; set; }

    }
}
