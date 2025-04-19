using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class ReactionPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required int ReactPost { get; set; }
        [BsonRequired]
        public required ObjectId PostId { get; set; }
        [BsonRequired]
        public required ObjectId CategoryReactionId { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? status { get; set; }

    }
}
