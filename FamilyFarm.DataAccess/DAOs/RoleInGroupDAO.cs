using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class RoleInGroupDAO
    {
        private readonly IMongoCollection<RoleInGroup> _RoleInGroups;

        public RoleInGroupDAO(IMongoDatabase database)
        {
            _RoleInGroups = database.GetCollection<RoleInGroup>("RoleInGroup");
        }

        public async Task<List<RoleInGroup>> GetAllAsync()
        {
            return await _RoleInGroups.Find(_ => true).ToListAsync();
        }

        public async Task<RoleInGroup> GetByIdAsync(string groupRoleId)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return null;

            return await _RoleInGroups.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
        }

        public async Task<RoleInGroup> CreateAsync(RoleInGroup roleInGroup)
        {
            roleInGroup.GroupRoleId = ObjectId.GenerateNewId().ToString();

            await _RoleInGroups.InsertOneAsync(roleInGroup);
            return roleInGroup;
        }

        public async Task<RoleInGroup> UpdateAsync(string groupRoleId, RoleInGroup updateRoleInGroup)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return null;

            var existing = await _RoleInGroups.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
            if (existing == null) return null;

            updateRoleInGroup.GroupRoleId = groupRoleId;
            await _RoleInGroups.ReplaceOneAsync(g => g.GroupRoleId == groupRoleId, updateRoleInGroup);
            return updateRoleInGroup;
        }

        public async Task<long> DeleteAsync(string groupRoleId)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return 0;

            var existing = await _RoleInGroups.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
            if (existing == null) return 0;

            var deleteResult = await _RoleInGroups.DeleteOneAsync(g => g.GroupRoleId == groupRoleId);

            return deleteResult.DeletedCount;
        }
    }
}
