using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllRole();
        Task<Role> GetRoleById(string role_id);
    }
}
