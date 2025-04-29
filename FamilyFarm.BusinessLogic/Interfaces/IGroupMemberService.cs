using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IGroupMemberService
    {
        Task<GroupMember> GetGroupMemberById(string groupMemberId);
        Task<GroupMember> AddGroupMember(GroupMember groupMember);
        Task<long> DeleteGroupMember(string groupMemberId);
    }
}
