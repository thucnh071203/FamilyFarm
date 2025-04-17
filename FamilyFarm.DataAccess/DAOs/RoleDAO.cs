using FamilyFarm.DataAccess.Context;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class RoleDAO : SingletonBase
    {
        private readonly IMongoCollection<Role> _Roles;

        public RoleDAO(IMongoDatabase database)
        {
            _Roles = database.GetCollection<Role>("Role");
        }

        public async Task<List<Role>> GetAllAsync()
        {
           return await _Roles.Find(_ => true).ToListAsync();
        }
            
        public async Task<Role?> GetByIdAsync(string role_id)
        {
            if (ObjectId.TryParse(role_id, out ObjectId objectRoleId))
            {
                return await _Roles.Find(r => r.RoleId == objectRoleId).FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task CreateAsync(Role Role)
        {
            await _Roles.InsertOneAsync(Role);
        }
            
        public async Task UpdateAsync(string role_id, Role role)
        {
            if (ObjectId.TryParse(role_id, out ObjectId objectRoleId))
            {
                await _Roles.ReplaceOneAsync(p => p.RoleId == objectRoleId, role);
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(role_id));
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objectRoleId))
            {
                await _Roles.DeleteOneAsync(p => p.RoleId == objectRoleId);
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(id));
            }
        }
    }
}

