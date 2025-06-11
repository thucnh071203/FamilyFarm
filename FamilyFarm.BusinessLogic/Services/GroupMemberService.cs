using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class GroupMemberService : IGroupMemberService
    {
        private readonly IGroupMemberRepository _groupMemberRepository;

        public GroupMemberService(IGroupMemberRepository groupMemberRepository)
        {
            _groupMemberRepository = groupMemberRepository;
        }

        public async Task<GroupMember> GetGroupMemberById(string groupMemberId)
        {
            return await _groupMemberRepository.GetGroupMemberById(groupMemberId);
        }

        public async Task<GroupMember> AddGroupMember(string groupId, string accountId, string inviterId)
        {
            return await _groupMemberRepository.AddGroupMember(groupId, accountId, inviterId);
        }

        public async Task<long> DeleteGroupMember(string groupMemberId)
        {
            return await _groupMemberRepository.DeleteGroupMember(groupMemberId);
        }
        public async Task<List<Account>> GetUsersInGroupAsync(string groupId)
        {
            return await _groupMemberRepository.GetUsersInGroupAsync(groupId);
        }
        public async Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword)
        {
            return await _groupMemberRepository.SearchUsersInGroupAsync(groupId, keyword);
        }
   
        public async Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId)
        {
            return await _groupMemberRepository.GetJoinRequestsAsync(groupId);
        }

        public async Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId)
        {
            return await _groupMemberRepository.RequestToJoinGroupAsync(accId, groupId);
        }
        public async Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus)
        {

            return await _groupMemberRepository.RespondToJoinRequestAsync(groupMemberId, responseStatus);
        }
        public async Task<bool> UpdateMemberRoleAsync(string groupId, string accId, string newGroupRoleId)
        {
            return await _groupMemberRepository.UpdateMemberRoleAsync(groupId, accId, newGroupRoleId);
        }

    }
}
