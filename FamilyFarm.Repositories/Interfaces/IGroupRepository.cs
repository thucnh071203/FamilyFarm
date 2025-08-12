using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroup();
        Task<Group> GetGroupById(string groupId);
        Task<Group> CreateGroup(Group item);
        Task<Group> UpdateGroup(string groupId, Group item);
        Task<long> DeleteGroup(string groupId);
        Task<Group> GetLatestGroupByCreator(string creatorId);
        Task<List<GroupCardDTO>> GetAllByUserId(string userId);
        Task<List<GroupCardDTO>> GetGroupsSuggestion(string userId, int number);
        Task<List<string>> GetGroupIdsByUserId(string accId);
        Task<List<GroupCardDTO>> SearchGroups(string userId, string searchTerm);
    }
}
