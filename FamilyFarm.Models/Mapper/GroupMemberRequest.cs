using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class GroupMemberRequest
    {
        public string GroupMemberId { get; set; }
        public string GroupId { get; set; }
        public string AccId { get; set; }
        public DateTime JointAt { get; set; }
        public string MemberStatus { get; set; }
        public string? InviteByAccId { get; set; }
        public DateTime? LeftAt { get; set; }
        public string AccountFullName { get; set; }
        public string AccountAvatar { get; set; }
    }
}
