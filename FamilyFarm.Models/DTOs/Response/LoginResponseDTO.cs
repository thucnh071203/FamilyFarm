using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class LoginResponseDTO
    {
        public string? Username { get; set; }
        public string? AccessToken { get; set; }
        public int TokenExpiryIn { get; set; }
        public string? RefreshToken { get; set; }
    }
}
