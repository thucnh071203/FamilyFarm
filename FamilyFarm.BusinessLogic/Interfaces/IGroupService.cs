using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IGroupService
    {
        Task<GroupResponseDTO> GetAllGroup();
        Task<GroupResponseDTO> GetGroupById(string groupId);
        Task<GroupResponseDTO> CreateGroup(GroupRequestDTO item);
        Task<GroupResponseDTO> UpdateGroup(string groupId, GroupRequestDTO item);
        Task<GroupResponseDTO> GetLatestGroupByCreator(string creatorId);
        Task<GroupResponseDTO> DeleteGroup(string groupId);
        Task<GroupCardResponseDTO> GetAllByUserId(string userId);
        Task<GroupCardResponseDTO> GetGroupSuggestion(string userId, int number);
        Task<GroupCardResponseDTO> SearchGroups(string userId, string searchTerm);
    }
}
