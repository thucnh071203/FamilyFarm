using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class CategoryPostDAO : SingletonBase
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryPostDAO(IMongoDatabase database)
        {
            _categoryCollection = database.GetCollection<Category>("Category");
        }

        public async Task<Category?> GetCategoryById(string? category_id)
        {
            if (string.IsNullOrEmpty(category_id))
                return null;

            var category = await _categoryCollection
                .Find(c => c.CategoryId == category_id && c.IsDeleted != true)
                .FirstOrDefaultAsync();

            return category;
        }
        /// <summary>
        /// get list category of post
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>?> GetListCategory()
        {
            var category = await _categoryCollection.Find(c => c.IsDeleted != true).ToListAsync();
            if (category == null || category.Count == 0) return null;
            return category;
        }
        public async Task<Category?> Create(Category category)
        {
            if (category == null) return null;
            try
            {
                category.CreateAt = DateTime.UtcNow;
                category.IsDeleted = false;
                await _categoryCollection.InsertOneAsync(category);
                return category;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> Delete(string categoryId)
        {
            if (categoryId == null) return false;
           
                var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId);
                var update = Builders<Category>.Update.Set(c => c.IsDeleted, true);

                await _categoryCollection.UpdateOneAsync(filter, update);
                return true;
           
        }
        public async Task<bool> Update(Category category)
        {
            if (category == null) return false;

            var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, category.CategoryId);
            var update = Builders<Category>.Update.Set(c => c.CategoryName, category.CategoryName)
                .Set(c => c.CategoryDescription, category.CategoryDescription)
                .Set(c => c.AccId, category.AccId)
                .Set(c => c.UpdateAt, category.UpdateAt);

            await _categoryCollection.UpdateOneAsync(filter, update);
            return true;

        }
    }
}
