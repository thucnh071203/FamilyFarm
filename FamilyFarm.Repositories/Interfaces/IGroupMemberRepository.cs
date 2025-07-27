using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember> GetGroupMemberById(string groupMemberId);
        Task<GroupMember> AddGroupMember(string groupId, string accountId, string inviterId);
        Task<GroupMember> AddGroupOwner(string groupId, string accountId);
        Task<long> DeleteGroupMember(string groupMemberId);
        Task<long> DeleteAllGroupMember(string groupId);


        Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId);

        Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword);

        Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId);
        Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId);

        Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus);

        Task<bool> UpdateMemberRoleAsync(string groupMemberId, string newGroupRoleId);
        Task<bool> LeaveGroupAsync(string groupId, string accId);
        Task<GroupMember> GetMemberJoinedGroup(string groupId, string accId);
        Task<GroupMember> InviteMember(string groupId, string accountId, string inviterId);
        Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus);
        Task<GroupMember> GetMemberInvitedGroup(string groupId, string accId);
        Task<GroupMember> GetMemberInvitedOrJoinedGroup(string groupId, string accId);
        Task<GroupMember> GetGroupMemberInviteById(string groupMemberId);
    }
}
