using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ReactionDAO
    {
        private readonly IMongoCollection<Reaction> _reactions;

        public ReactionDAO(IMongoDatabase database)
        {
            _reactions = database.GetCollection<Reaction>("Reaction");
        }

        public async Task<Reaction> GetByEntityAccAndReactionAsync(string entityId, string entityType, string accId, string categoryReactionId)
        {
            if (!ObjectId.TryParse(entityId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
                return null;
            return await _reactions
                .Find(r => r.EntityId == entityId && r.EntityType == entityType && r.AccId == accId && r.CategoryReactionId == categoryReactionId)
                .FirstOrDefaultAsync();
        }

        public async Task<Reaction> GetByEntityAndAccAsync(string entityId, string entityType, string accId)
        {
            if (!ObjectId.TryParse(entityId, out _) || !ObjectId.TryParse(accId, out _))
                return null;
            return await _reactions
                .Find(r => r.EntityId == entityId && r.EntityType == entityType && r.AccId == accId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Reaction>> GetAllByEntityAsync(string entityId, string entityType)
        {
            if (!ObjectId.TryParse(entityId, out _))
                return new List<Reaction>();
            return await _reactions
                .Find(r => r.EntityId == entityId && r.EntityType == entityType && r.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<Reaction> CreateAsync(Reaction reaction)
        {
            reaction.ReactionId = ObjectId.GenerateNewId().ToString();
            reaction.CreateAt = DateTime.UtcNow;
            reaction.UpdateAt = DateTime.UtcNow;
            reaction.IsDeleted = false;
            await _reactions.InsertOneAsync(reaction);
            return reaction;
        }

        public async Task<bool> UpdateAsync(string reactionId, string categoryReactionId, bool isDeleted)
        {
            var filter = Builders<Reaction>.Filter.Where(r => r.ReactionId == reactionId);
            var update = Builders<Reaction>.Update
                .Set(r => r.CategoryReactionId, categoryReactionId)
                .Set(r => r.IsDeleted, isDeleted)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactions.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string reactionId)
        {
            var filter = Builders<Reaction>.Filter.Where(r => r.ReactionId == reactionId);
            var update = Builders<Reaction>.Update
                .Set(r => r.IsDeleted, true)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactions.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RestoreAsync(string reactionId)
        {
            var filter = Builders<Reaction>.Filter.Where(r => r.ReactionId == reactionId);
            var update = Builders<Reaction>.Update
                .Set(r => r.IsDeleted, false)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactions.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
