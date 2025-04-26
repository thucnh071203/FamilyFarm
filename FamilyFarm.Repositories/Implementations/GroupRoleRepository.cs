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
    public class GroupRoleRepository : IGroupRoleRepository
    {
        private readonly GroupRoleDAO _dao;
        public GroupRoleRepository(GroupRoleDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<GroupRole>> GetAllGroupRole()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<GroupRole> GetGroupRoleById(string groupRoleId)
        {
            return await _dao.GetByIdAsync(groupRoleId);
        }

        public async Task<GroupRole> CreateGroupRole(GroupRole item)
        {
            return await _dao.CreateAsync(item);
        } 

        public async Task<GroupRole> UpdateGroupRole(string groupRoleId, GroupRole item)
        {
            return await _dao.UpdateAsync(groupRoleId, item);
        }

        public async Task<GroupRole> DeleteGroupRole(string groupRoleId)
        {
            return await _dao.DeleteAsync(groupRoleId);
        }
    }
}
