using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ChatDTO
    {
        public string ChatId { get; set; }
        public string Acc1Id { get; set; }
        public string Acc2Id { get; set; }
        public DateTime? CreateAt { get; set; }
        public string LastMessage { get; set; } // Tin nhắn cuối cùng
        public int UnreadCount { get; set; } // Số tin nhắn chưa đọc
    }
}
