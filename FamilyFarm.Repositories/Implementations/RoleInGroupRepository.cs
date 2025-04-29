using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class RoleInGroupRepository : IRoleInGroupRepository
    {
        private readonly RoleInGroupDAO _dao;
        public RoleInGroupRepository(RoleInGroupDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<RoleInGroup>> GetAllRoleInGroup()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<RoleInGroup> GetRoleInGroupById(string groupRoleId)
        {
            return await _dao.GetByIdAsync(groupRoleId);
        }

        public async Task<RoleInGroup> CreateRoleInGroup(RoleInGroup item)
        {
            return await _dao.CreateAsync(item);
        }

        public async Task<RoleInGroup> UpdateRoleInGroup(string groupRoleId, RoleInGroup item)
        {
            return await _dao.UpdateAsync(groupRoleId, item);
        }

        public async Task<long> DeleteRoleInGroup(string groupRoleId)
        {
            return await _dao.DeleteAsync(groupRoleId);
        }
    }
}
