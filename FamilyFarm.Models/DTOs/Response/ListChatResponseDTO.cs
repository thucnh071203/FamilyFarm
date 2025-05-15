using FamilyFarm.Models.DTOs.EntityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListChatResponseDTO
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public int? unreadChatCount { get; set; } // số đoạn chat chưa đọc
        public List<ChatDTO> Chats { get; set; } = new List<ChatDTO>();
    }
}
