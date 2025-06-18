using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class LoginResponseDTO
    {
        public string? AccId { get; set; }
        public string? Username { get; set; }
        public string? RoleId { get; set; }
        public string? AccessToken { get; set; }
        public int TokenExpiryIn { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
