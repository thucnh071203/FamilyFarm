using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Models
{
    public class SubProcess
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SubprocessId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? FarmerId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ExpertId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BookingServiceId {  get; set; } //Sub process xử lý cho booking nào
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProcessId { get; set; } //Copy từ process nào
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int NumberOfSteps { get; set; }
        public int ContinueStep { get; set; }
        public string? SubProcessStatus { get; set; } //Trạng thái của sub process: đã hoàn thành hoặc chưa
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CompleteAt { get; set; }
        public bool? IsCompletedByFarmer { get; set; }
        public bool? IsDeleted {  get; set; }
        public decimal? Price { get; set; } //Dành cho các sub process thêm cho farmer
        public bool? IsAccept {  get; set; } //Kiểm tra người farmer có chấp nhận thêm process hay không
        public DateTime? PayAt { get; set; } //Lưu thời gian khi người dùng trả tiền thêm cho process thêm
        public bool? IsExtraProcess { get; set; } //Xem có phải là process thêm khi giải quyết thêm cho farmer hay không
    }
}
