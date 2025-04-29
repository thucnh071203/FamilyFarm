using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IRoleInGroupRepository
    {
        Task<List<RoleInGroup>> GetAllRoleInGroup();
        Task<RoleInGroup> GetRoleInGroupById(string groupRoleId);
        Task<RoleInGroup> CreateRoleInGroup(RoleInGroup item);
        Task<RoleInGroup> UpdateRoleInGroup(string groupRoleId, RoleInGroup item);
        Task<long> DeleteRoleInGroup(string groupRoleId);
    }
}
