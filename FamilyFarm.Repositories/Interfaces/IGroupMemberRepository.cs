using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember> GetGroupMemberById(string groupMemberId);
        Task<GroupMember> AddGroupMember(GroupMember groupMember);
        Task<long> DeleteGroupMember(string groupMemberId);
        Task<long> DeleteAllGroupMember(string groupId);
    }
}
