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
    public class GroupDAO
    {
        private readonly IMongoCollection<Group> _Groups;

        public GroupDAO(IMongoDatabase database)
        {
            _Groups = database.GetCollection<Group>("Group");
        }

        public async Task<List<Group>> GetAllAsync()
        {
            return await _Groups.Find(g => g.IsDeleted != true).ToListAsync();
        }

        public async Task<Group> GetByIdAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            return await _Groups.Find(g => g.GroupId == groupId && g.IsDeleted != true).FirstOrDefaultAsync();
        }

        public async Task<Group> CreateAsync(Group group)
        {
            group.GroupId = ObjectId.GenerateNewId().ToString();
            group.CreatedAt = DateTime.UtcNow;
            group.UpdatedAt = null;
            group.DeletedAt = null;
            group.IsDeleted = false;
            await _Groups.InsertOneAsync(group);
            return group;
        }

        public async Task<Group> UpdateAsync(string groupId, Group updateGroup)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            var filter = Builders<Group>.Filter.Eq(g => g.GroupId, groupId);

            if (filter == null) return null;

            var update = Builders<Group>.Update
                .Set(g => g.GroupName, updateGroup.GroupName)
                .Set(g => g.GroupAvatar, updateGroup.GroupAvatar)
                .Set(g => g.GroupBackground, updateGroup.GroupBackground)
                .Set(g => g.PrivacyType, updateGroup.PrivacyType)
                .Set(g => g.UpdatedAt, DateTime.UtcNow);

            var result = await _Groups.UpdateOneAsync(filter, update);
            return updateGroup;
        }

        public async Task<long> DeleteAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return 0;

            var filter = Builders<Group>.Filter.Eq(g => g.GroupId, groupId);

            var update = Builders<Group>.Update
                .Set(g => g.DeletedAt, DateTime.UtcNow)
                .Set(g => g.IsDeleted, true);

            var result = await _Groups.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }

        public async Task<Group> GetLatestByCreatorAsync(string creatorId)
        {
            if (!ObjectId.TryParse(creatorId, out _)) return null;

            return await _Groups.Find(g => g.OwnerId == creatorId && g.IsDeleted != true)
                                .SortByDescending(g => g.CreatedAt)
                                .FirstOrDefaultAsync();
        }

    }
}
