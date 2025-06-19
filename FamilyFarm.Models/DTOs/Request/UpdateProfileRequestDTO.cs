using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdateProfileRequestDTO
    {
        public required string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public string? Address { get; set; }
        public IFormFile? Background { get; set; }
        public IFormFile? Avatar { get; set; }
        // Expert
        public IFormFile? Certificate { get; set; }
        public string? WorkAt { get; set; }
        public string? StudyAt { get; set; }
    }
}
