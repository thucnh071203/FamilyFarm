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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CategoryReaction> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return null;
            return await _categoryReactions
                .Find(c => c.CategoryReactionId == id && c.IsDeleted != true)
                .FirstOrDefaultAsync();
        }
    }
}
