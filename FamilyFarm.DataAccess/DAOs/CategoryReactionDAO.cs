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
    public class CategoryReactionDAO
    {
        private readonly IMongoCollection<CategoryReaction> _categoryReactions;

        public CategoryReactionDAO(IMongoDatabase database)
        {
            _categoryReactions = database.GetCollection<CategoryReaction>("CategoryReaction");
        }

        /// <summary>
        /// Retrieves all CategoryReactions.
        /// </summary>
        /// <returns>A list of all CategoryReactions.</returns>
        public async Task<List<CategoryReaction>> GetAllAsync()
        {
            return await _categoryReactions.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Retrieves a CategoryReaction by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the CategoryReaction.</param>
        /// <returns>The CategoryReaction if found, null otherwise.</returns>
        public async Task<CategoryReaction> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return null;
            return await _categoryReactions
                .Find(c => c.CategoryReactionId == id && c.IsDeleted != true)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CategoryReaction reaction)
        {
            await _categoryReactions.InsertOneAsync(reaction);
        }

        public async Task<bool> UpdateAsync(string id, CategoryReaction updatedReaction)
        {
            var result = await _categoryReactions.ReplaceOneAsync(
                r => r.CategoryReactionId == id && r.IsDeleted != true,
                updatedReaction
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var update = Builders<CategoryReaction>.Update.Set(r => r.IsDeleted, true);
            var result = await _categoryReactions.UpdateOneAsync(
                r => r.CategoryReactionId == id && r.IsDeleted != true,
                update
            );
            return result.ModifiedCount > 0;
        }
        public async Task<bool> RestoreAsync(string id)
        {
            var update = Builders<CategoryReaction>.Update.Set(r => r.IsDeleted, false);
            var result = await _categoryReactions.UpdateOneAsync(
                r => r.CategoryReactionId == id && r.IsDeleted != false,
                update
            );
            return result.ModifiedCount > 0;
        }


    }
}
