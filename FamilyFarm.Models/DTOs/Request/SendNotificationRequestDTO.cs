using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class SendNotificationRequestDTO
    {
        public List<string> ReceiverIds { get; set; }
        public string? SenderId { get; set; }
        public string? CategoryNotiId { get; set; }
        public string? TargetId { get; set; }
        public string? TargetType { get; set; }
        public string? Content { get; set; }
    }
}
