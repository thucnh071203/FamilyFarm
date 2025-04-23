using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class RegisterFarmerResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string? MessageError { get; set; }
    }
}
