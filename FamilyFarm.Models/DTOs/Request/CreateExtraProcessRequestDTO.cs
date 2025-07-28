using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreateExtraProcessRequestDTO
    {
        public string? BookingId { get; set; }
        public string? ExtraDescription { get; set; }
    }
}
