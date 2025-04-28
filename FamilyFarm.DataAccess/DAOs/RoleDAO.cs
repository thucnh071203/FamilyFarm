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

        public async Task<Role> GetByIdAsync(string? role_id)
        {
            if (!string.IsNullOrEmpty(role_id) && ObjectId.TryParse(role_id, out _))
            {
                return await _Roles.Find(r => r.RoleId == role_id).FirstOrDefaultAsync();
            }
            return null; // Return null if role_id is invalid
        }

        public async Task<bool> CreateAsync(Role role)
        {
            if (role == null)
            {
                return false; // Return false if the role is null.
            }

            await _Roles.InsertOneAsync(role);
            return true;
        }

        public async Task<bool> UpdateAsync(string role_id, Role role)
        {
            if (string.IsNullOrEmpty(role_id) || role == null || !ObjectId.TryParse(role_id, out _))
            {
                return false; // Return false if the role_id is invalid or ObjectId format is incorrect.
            }

            var result = await _Roles.ReplaceOneAsync(r => r.RoleId == role_id, role);
            return result.ModifiedCount > 0; // Return true if something was modified.
        }

        public async Task<bool> DeleteAsync(string? id)
        {
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return false; // Return false if the id is invalid or ObjectId format is incorrect.
            }

            var result = await _Roles.DeleteOneAsync(r => r.RoleId == id);
            return result.DeletedCount > 0; // Return true if something was deleted.
        }
    }
}



