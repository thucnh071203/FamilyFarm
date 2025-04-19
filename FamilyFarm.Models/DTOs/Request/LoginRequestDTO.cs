using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class LoginRequestDTO
    {
        public string? Identifier { get; set; }
        public string? Password { get; set; }
    }
}
