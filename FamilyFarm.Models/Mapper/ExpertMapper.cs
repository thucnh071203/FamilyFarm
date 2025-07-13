using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class ExpertMapper
    {
        public string? AccId { get; set; }
        public string? RoleId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Background { get; set; }
        public string? Certificate { get; set; }
        public string? WorkAt { get; set; }
        public string? StudyAt { get; set; }
        public int? Status { get; set; }
        public string? FriendStatus { get; set; }
        public int? MutualFriend { get; set; }
    }
}
