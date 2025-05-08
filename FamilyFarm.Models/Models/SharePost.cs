using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class SharePost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SharePostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }
        public string SharePostContent { get; set; }
        public string? SharePostScope { get; set; } // Public, Private, Draft,...
        public int Status { get; set; } //1 la khi content khong lien quan den nong nghiep, 0 la bth
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
