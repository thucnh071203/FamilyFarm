using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ConfirmSubprocessRequestDTO
    {
        public string? SubprocessId { get; set; }
        public string? BookingServiceid { get; set; }
    }
}
