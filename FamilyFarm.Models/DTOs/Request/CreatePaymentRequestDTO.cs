using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreatePaymentRequestDTO
    {
        public string? BookingServiceId { get; set; }
        public string? SubprocessId { get; set; }
        public decimal? Amount { get; set; }
    }
}
