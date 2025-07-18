using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ReviewSummaryDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingCounts { get; set; } // ví dụ: {5: 50, 4: 20, ...}
    }
}
