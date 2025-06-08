using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IGroupService
    {
        Task<List<Group>> GetAllGroup();
        Task<Group> GetGroupById(string groupId);
        Task<Group> CreateGroup(Group item);
        Task<Group> UpdateGroup(string groupId, Group item);
        Task<long> DeleteGroup(string groupId);
        Task<List<Group>> GetAllByUserId(string userId);
    }
}
