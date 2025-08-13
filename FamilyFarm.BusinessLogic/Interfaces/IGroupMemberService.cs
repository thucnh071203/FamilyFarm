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
        Task<GroupMember> AddGroupMember(string groupId, string accountId, string inviterId);
        Task<long> DeleteGroupMember(string groupMemberId, string deleterID);
        Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId, string accId);
        Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword);
        Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId);
        Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId);
        Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus);
        Task<bool> UpdateMemberRoleAsync(string groupMemberId, string newGroupRoleId);
        Task<GroupMemberResponseDTO> GetOneUserInGroupAsync(string groupId, string accId);
        Task<bool> LeaveGroupAsync(string groupId, string accId);
        Task<GroupMember> InviteGroupMember(string groupId, string accountId, string inviterId);
        //Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus);
        //Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus, out var message);
        Task<(bool Success, string? Message)> RespondToInviteRequestAsync(string groupMemberId, string responseStatus);
        Task<GroupMember> GetMemberInvitedOrJoinedGroup(string groupId, string accId);
        Task<GroupMember> GetGroupMemberInviteById(string groupMemberId);
    }
}
