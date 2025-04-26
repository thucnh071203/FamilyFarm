using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
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

        public async Task<GroupMember> DeleteGroupMember(string groupMemberId)
        {
            return await _groupMemberRepository.DeleteGroupMember(groupMemberId);
        }
    }
}
