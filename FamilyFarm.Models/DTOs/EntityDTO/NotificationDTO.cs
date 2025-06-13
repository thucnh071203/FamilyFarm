using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class NotificationDTO
    {
        public string NotifiId { get; set; } 
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string CategoryNotifiId { get; set; } 
        public string CategoryName { get; set; }
        
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }

        public string TargetId { get; set; }
        public string? TargetType { get; set; }
        public string? TargetContent { get; set; }

        public NotificationStatus? Status { get; set; }
    }
}
