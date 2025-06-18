using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class MemberActivityResponseDTO
    {
        public string AccId { get; set; }
        public string RoleName { get; set; }
        public string AccountName { get; set; }
        public string AccountAddress { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPayments { get; set; }
        public int TotalActivity { get; set; } // Tổng hoạt động (posts + comments + bookings + payments)

    }
}
