using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class RevenueSystemDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalCommission { get; set; }
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    }


   


}
