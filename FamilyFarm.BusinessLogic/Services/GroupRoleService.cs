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
    public class GroupRoleService : IGroupRoleService
    {
        private readonly IGroupRoleRepository _groupRoleRepository;

        public GroupRoleService(IGroupRoleRepository groupRoleRepository)
        {
            _groupRoleRepository = groupRoleRepository;
        }

        public async Task<List<GroupRole>> GetAllGroupRole()
        {
            return await _groupRoleRepository.GetAllGroupRole();
        }

        public async Task<GroupRole> GetGroupRoleById(string groupRoleId)
        {
            return await _groupRoleRepository.GetGroupRoleById(groupRoleId);
        }

        public async Task<GroupRole> CreateGroupRole(GroupRole item)
        {
            return await _groupRoleRepository.CreateGroupRole(item);
        }

        public async Task<GroupRole> UpdateGroupRole(string groupRoleId, GroupRole item)
        {
            return await _groupRoleRepository.UpdateGroupRole(groupRoleId, item);
        }

        public async Task<GroupRole> DeleteGroupRole(string groupRoleId)
        {
            return await _groupRoleRepository.DeleteGroupRole(groupRoleId);
        }
    }
}
