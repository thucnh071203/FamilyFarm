using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.Response
{
    public class RepaymentResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public PaymentTransaction Data { get; set; }
    }
}
