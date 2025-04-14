using FamilyFarm.DataAccess.Context;
using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class RoleDAO : SingletonBase
    {
       private readonly IMongoCollection<Role> _Roles;

        public RoleDAO()
        {

            _Roles = Database?.GetCollection<Role>("Role");
        }

        public async Task<List<Role>> GetAllAsync() =>
            await _Roles.Find(_ => true).ToListAsync();

        public async Task<Role?> GetByIdAsync(string id) =>
            await _Roles.Find(p => p.RoleId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Role Role) =>
            await _Roles.InsertOneAsync(Role);

        public async Task UpdateAsync(string id, Role Role) =>
            await _Roles.ReplaceOneAsync(p => p.RoleId == id, Role);

        public async Task DeleteAsync(string id) =>
            await _Roles.DeleteOneAsync(p => p.RoleId == id);
    }
}

