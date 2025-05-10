using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleDAO _roleDAO;

        public RoleRepository(RoleDAO roleDAO)
        {
            _roleDAO = roleDAO;
        }

        public async Task<List<Role>> GetAllRole()
        {
            return await _roleDAO.GetAllAsync();
        }

        public async Task<Role> GetRoleById(string role_id)
        {
            return await _roleDAO.GetByIdAsync(role_id);
        }


    }
}
