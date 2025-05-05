using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class GroupRequestDTO
    {
        public required string AccountId { get; set; }
        public required string GroupName { get; set; }
        public string? GroupAvatar { get; set; }
        public string? GroupBackground { get; set; }
        public required string PrivacyType { get; set; }
    }
}
