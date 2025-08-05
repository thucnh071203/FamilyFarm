using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreditCardUpdateRequestDTO
    {
        public string? CreditNumber { get; set; }
        public string? CreditName { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
