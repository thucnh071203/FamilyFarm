using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class GroupRequestDTO
    {
        public string? AccountId { get; set; }
        //public string? GroupName { get; set; }
        public required string GroupName { get; set; }
        public IFormFile? GroupAvatar { get; set; }
        public IFormFile? GroupBackground { get; set; }
        //public string? PrivacyType { get; set; }
        public required string PrivacyType { get; set; }
    }
}
