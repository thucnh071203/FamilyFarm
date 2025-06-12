using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class GroupMemberResponseDTO
    {
        public string GroupMemberId { get; set; }
        public string GroupId { get; set; }
        public string AccId { get; set; }
        public DateTime JointAt { get; set; }
        public string MemberStatus { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string City { get; set; }
        public string RoleInGroupId { get; set; }
    }
}
