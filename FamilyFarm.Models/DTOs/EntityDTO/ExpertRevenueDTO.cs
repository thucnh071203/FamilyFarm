using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ExpertRevenueDTO
    {
        public string ExpertId { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal CommissionRevenue { get; set; }
        public int TotalServicesProvided { get; set; }
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, decimal> MonthlyCommission { get; set; } = new();
        public List<string> TopServiceNames { get; set; } = new();
        public Dictionary<string, decimal> DailyRevenue { get; set; }
        public Dictionary<string, decimal> DailyCommission { get; set; }

    }
}
