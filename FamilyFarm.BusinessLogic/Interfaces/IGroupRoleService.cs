using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IGroupRoleService
    {
        Task<List<GroupRole>> GetAllGroupRole();
        Task<GroupRole> GetGroupRoleById(string groupRoleId);
        Task<GroupRole> CreateGroupRole(GroupRole item);
        Task<GroupRole> UpdateGroupRole(string groupRoleId, GroupRole item);
        Task<GroupRole> DeleteGroupRole(string groupRoleId);
    }
}
