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

        public GroupService(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
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
            return await _groupRepository.CreateGroup(item);
        }

        public async Task<Group> UpdateGroup(string groupId, Group item)
        {
            return await _groupRepository.UpdateGroup(groupId, item);
        }

        public async Task<Group> DeleteGroup(string groupId)
        {
            return await _groupRepository.DeleteGroup(groupId);
        }
    }
}
