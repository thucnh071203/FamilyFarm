using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class LoginGoogleRequestDTO
    {
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
