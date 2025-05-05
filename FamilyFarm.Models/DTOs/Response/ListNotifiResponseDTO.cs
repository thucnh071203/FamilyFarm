using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListNotifiResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UnreadCount { get; set; }
        public List<Notification> Notifications { get; set; }
    }
}
