using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ReviewRequestDTO
    {
        public string? ServiceId { get; set; }
        public string? BookingServiceId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
