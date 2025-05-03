using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UserProfileResponseDTO
    {
        public string AccId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? WorkAt { get; set; }
        public string? StudyAt { get; set; }
    }
}
