using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListChatDetailsResponseDTO
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public long TotalMessages { get; set; } // Tổng số tin nhắn
        public List<ChatDetail> ChatDetails { get; set; } = new List<ChatDetail>();
    }
}
