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
    public class RoleInGroupService : IRoleInGroupService
    {
        private readonly IRoleInGroupRepository _roleInGroupRepository;

        public RoleInGroupService(IRoleInGroupRepository roleInGroupRepository)
        {
            _roleInGroupRepository = roleInGroupRepository;
        }

        public async Task<List<RoleInGroup>> GetAllRoleInGroup()
        {
            return await _roleInGroupRepository.GetAllRoleInGroup();
        }

        public async Task<RoleInGroup> GetRoleInGroupById(string groupRoleId)
        {
            return await _roleInGroupRepository.GetRoleInGroupById(groupRoleId);
        }

        public async Task<RoleInGroup> CreateRoleInGroup(RoleInGroup item)
        {
            return await _roleInGroupRepository.CreateRoleInGroup(item);
        }

        public async Task<RoleInGroup> UpdateRoleInGroup(string groupRoleId, RoleInGroup item)
        {
            return await _roleInGroupRepository.UpdateRoleInGroup(groupRoleId, item);
        }

        public async Task<long> DeleteRoleInGroup(string groupRoleId)
        {
            return await _roleInGroupRepository.DeleteRoleInGroup(groupRoleId);
        }
    }
}
