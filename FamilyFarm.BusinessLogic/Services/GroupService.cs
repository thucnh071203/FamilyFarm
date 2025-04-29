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
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _memberRepository;

        public GroupService(IGroupRepository groupRepository, IGroupMemberRepository memberRepository)
        {
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
        }

        public async Task<List<Group>> GetAllGroup()
        {
            return await _groupRepository.GetAllGroup();
        }

        public async Task<Group> GetGroupById(string groupId)
        {
            return await _groupRepository.GetGroupById(groupId);
        }

        public async Task<Group> CreateGroup(Group item)
        {
            var result = await _groupRepository.CreateGroup(item);

            if (result == null) return null;

            var getLastestGroup = await _groupRepository.GetLatestGroupByCreator(item.OwnerId);

            // Thêm người tạo group vào group
            var newOwnerMember = new GroupMember
            {
                GroupMemberId = null,
                GroupRoleId = "680ce8722b3eec497a30201e",
                GroupId = getLastestGroup.GroupId,
                AccId = item.OwnerId,
                JointAt = DateTime.UtcNow,
                MemberStatus = "Accept",
                InviteByAccId = null,
                LeftAt = null
            };

            var addOwnerToGroup = await _memberRepository.AddGroupMember(newOwnerMember);

            if (addOwnerToGroup == null) return null;

            return item;
        }

        public async Task<Group> UpdateGroup(string groupId, Group item)
        {
            return await _groupRepository.UpdateGroup(groupId, item);
        }

        public async Task<long> DeleteGroup(string groupId)
        {
            var result = await _groupRepository.DeleteGroup(groupId);

            if (result == null) return 0;

            // Xóa tất cả thành viên khi xóa group
            var deleteAllMember = await _memberRepository.DeleteAllGroupMember(groupId);

            if (deleteAllMember <= 0) return 0;

            return result;
        }
    }
}
