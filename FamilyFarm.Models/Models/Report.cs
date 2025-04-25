using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FamilyFarm.Models.Models
{
    public class Report
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ReportId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ReporterId { get; set; } // AccId của người report
        [BsonRepresentation(BsonType.ObjectId)]
        public required string PostId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? HandledById { get; set; } // Người xử lý (admin), chưa sử lý thì null
        public required string Reason { get; set; }
        public string Status { get; set; } // Trạng thái xử lý: pending, accepted, rejected, gì gì đó...
        public string? HandlerNote { get; set; } // Ghi chú xử lý của admin (nếu có)
        public DateTime CreatedAt { get; set; }
        public DateTime? HandledAt { get; set; } // Thời điểm xử lý
        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;
    }
}
