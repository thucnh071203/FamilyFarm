using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FamilyFarm.Models.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }
        public string? PostContent { get; set; }
        public string? PostScope { get; set; }
        [BsonRepresentation(BsonType.ObjectId)] 
        public string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? GroupId { get; set; }
        //public bool IsInGroup { get; set; } = false;
        public bool? IsInGroup { get; set; }
        public int Status { get; set; }//1 la khi content khong lien quan den nong nghiep, 0 la bth
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
