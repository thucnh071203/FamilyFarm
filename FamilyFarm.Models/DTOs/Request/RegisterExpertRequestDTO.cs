using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class RegisterExpertRequestDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Fullname { get; set; }
        public IFormFile? Avatar {  get; set; }
        [Required]
        public string Email { get; set; }
        [Required]

        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        [Required]
        public string Identifier { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public IFormFile Certificate { get; set; }

    }
}
