using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
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

        public async Task<List<GroupCardDTO>> GetAllByUserId(string userId)
        {
            return await _dao.GetAllByUserId(userId);
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

        public async Task<long> DeleteGroup(string groupId)
        {
            return await _dao.DeleteAsync(groupId);
        }
        public async Task<Group> GetLatestGroupByCreator(string creatorId)
        {
            return await _dao.GetLatestByCreatorAsync(creatorId);
        }

        public async Task<List<GroupCardDTO>> GetGroupsSuggestion(string userId, int number)
        {
            return await _dao.GetGroupsSuggestion(userId, number);
        }
        public async Task<List<string>> GetGroupIdsByUserId(string accId)
        {
            return await _dao.GetGroupIdsByUserId(accId);
        }
        public async Task<List<GroupCardDTO>> SearchGroups(string userId, string searchTerm)
        {
            return await _dao.SearchGroups(userId, searchTerm);
        }
    }
}
