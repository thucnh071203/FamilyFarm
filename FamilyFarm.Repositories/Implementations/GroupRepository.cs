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
    public class GroupRepository : IGroupRepository
    {
        private readonly GroupDAO _dao;
        public GroupRepository(GroupDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<Group>> GetAllGroup()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<Group> GetGroupById(string groupId)
        {
            return await _dao.GetByIdAsync(groupId);
        }

        public async Task<Group> CreateGroup(Group item)
        {
            return await _dao.CreateAsync(item);
        } 

        public async Task<Group> UpdateGroup(string groupId, Group item)
        {
            return await _dao.UpdateAsync(groupId, item);
        }

        public async Task<Group> DeleteGroup(string groupId)
        {
            return await _dao.DeleteAsync(groupId);
        }
    }
}
