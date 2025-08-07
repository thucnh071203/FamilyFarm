using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly GroupMemberDAO _dao;
        public GroupMemberRepository(GroupMemberDAO dao)
        {
            _dao = dao;
        }

        public async Task<GroupMember> GetGroupMemberById(string groupMemberId)
        {
            return await _dao.GetByIdAsync(groupMemberId);
        }

        public async Task<GroupMember> AddGroupMember(string groupId, string accountId, string inviterId)
        {
            return await _dao.AddAsync(groupId, accountId, inviterId);
        }

        public async Task<GroupMember> AddGroupOwner(string groupId, string accountId)
        {
            return await _dao.AddOwnersync(groupId, accountId);
        }

        public async Task<long> DeleteGroupMember(string groupMemberId)
        {
            return await _dao.DeleteAsync(groupMemberId);
        }

        public async Task<long> DeleteAllGroupMember(string groupId)
        {
            return await _dao.DeleteAllAsync(groupId);
        }


        public async Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId)

        {
            return await _dao.GetUsersInGroupAsync(groupId);
        }
        public async Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword)
        {
            return await _dao.SearchUsersInGroupAsync(groupId, keyword);
        }
  
        public async Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId)
        {
            return await _dao.GetJoinRequestsAsync(groupId);
        }
        public async Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId)
        {
            return await _dao.RequestToJoinGroupAsync(accId, groupId);
        }

        public async Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus)
        {
            return await _dao.RespondToJoinRequestAsync(groupMemberId, responseStatus);
        }

        public async Task<bool> UpdateMemberRoleAsync(string groupMemberId, string newGroupRoleId)
        {
            return await _dao.UpdateRoleAsync(groupMemberId, newGroupRoleId);
        }
        public async Task<bool> LeaveGroupAsync(string groupId, string accId)
        {
            return await _dao.LeaveGroupAsync(groupId, accId);
        }

        public async Task<GroupMember> GetMemberJoinedGroup(string groupId, string accId)
        {
            return await _dao.GetMemberJoinedGroupAsync(groupId, accId);
        }

        public async Task<GroupMember> InviteMember(string groupId, string accountId, string inviterId)
        {
            return await _dao.InviteAsync(groupId, accountId, inviterId);
        }

        public async Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus)
        {
            return await _dao.RespondToInviteRequestAsync(groupMemberId, responseStatus);
        }

        public async Task<GroupMember> GetMemberInvitedGroup(string groupId, string accId)
        {
            return await _dao.GetMemberInvitedGroupAsync(groupId, accId);
        }

        public async Task<GroupMember> GetMemberInvitedOrJoinedGroup(string groupId, string accId)
        {
            return await _dao.GetMemberInvitedOrJoinedGroupAsync(groupId, accId);
        }

        public async Task<GroupMember> GetGroupMemberInviteById(string groupMemberId)
        {
            return await _dao.GetMemberInviteByIdAsync(groupMemberId);
        }
    }
}
