using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class GroupRoleDAO
    {
        private readonly IMongoCollection<GroupRole> _GroupRoles;

        public GroupRoleDAO(IMongoDatabase database)
        {
            _GroupRoles = database.GetCollection<GroupRole>("GroupRole");
        }

        public async Task<List<GroupRole>> GetAllAsync()
        {
            return await _GroupRoles.Find(_ => true).ToListAsync();
        }

        public async Task<GroupRole> GetByIdAsync(string groupRoleId)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return null;

            return await _GroupRoles.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
        }

        public async Task<GroupRole> CreateAsync(GroupRole groupRole)
        {
            groupRole.GroupRoleId = ObjectId.GenerateNewId().ToString();

            await _GroupRoles.InsertOneAsync(groupRole);
            return groupRole;
        }

        public async Task<GroupRole> UpdateAsync(string groupRoleId, GroupRole updateGroupRole)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return null;

            var existing = await _GroupRoles.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
            if (existing == null) return null;

            updateGroupRole.GroupRoleId = groupRoleId;
            await _GroupRoles.ReplaceOneAsync(g => g.GroupRoleId == groupRoleId, updateGroupRole);
            return updateGroupRole;
        }

        public async Task<GroupRole> DeleteAsync(string groupRoleId)
        {
            if (!ObjectId.TryParse(groupRoleId, out _)) return null;

            var existing = await _GroupRoles.Find(g => g.GroupRoleId == groupRoleId).FirstOrDefaultAsync();
            if (existing == null) return null;

            var deleteResult = await _GroupRoles.DeleteOneAsync(g => g.GroupRoleId == groupRoleId);

            if (deleteResult.DeletedCount > 0)
            {
                return existing;
            }

            return null;
        }
    }
}
