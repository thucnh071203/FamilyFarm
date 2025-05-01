using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
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

        public async Task<GroupMember> AddGroupMember(GroupMember item)
        {
            return await _groupMemberRepository.AddGroupMember(item);
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

    }
}
