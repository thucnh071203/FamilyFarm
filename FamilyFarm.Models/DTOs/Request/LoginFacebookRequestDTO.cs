using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class LoginFacebookRequestDTO
    {
        public string FacebookId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Avatar { get; set; }
    }
}
