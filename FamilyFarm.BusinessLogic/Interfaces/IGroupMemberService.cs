using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IGroupMemberService
    {
        Task<GroupMember> GetGroupMemberById(string groupMemberId);
        Task<GroupMember> AddGroupMember(GroupMember groupMember);
        Task<long> DeleteGroupMember(string groupMemberId);
        Task<List<Account>> GetUsersInGroupAsync(string groupId);
        Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword);
        Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId);
        Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId);
        Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus);
    }
}
