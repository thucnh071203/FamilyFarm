using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class CategoryServiceDAO
    {
        private readonly IMongoCollection<CategoryService> _CategoryServices;

        public CategoryServiceDAO(IMongoDatabase database)
        {
            _CategoryServices = database.GetCollection<CategoryService>("CategoryService");
        }

        public async Task<List<CategoryService>> GetAllAsync()
        {
            return await _CategoryServices.Find(g => g.IsDeleted != true).ToListAsync();
        }
        public async Task<List<CategoryService>> GetAllForAdmin()
        {
            return await _CategoryServices.Find(_ => true).ToListAsync();
        }

        public async Task<CategoryService> GetByIdAsync(string categoryServiceId)
        {
            if (!ObjectId.TryParse(categoryServiceId, out _)) return null;

            return await _CategoryServices.Find(g => g.CategoryServiceId == categoryServiceId && g.IsDeleted != true).FirstOrDefaultAsync();
        }

        public async Task<CategoryService> CreateAsync(CategoryService categoryService)
        {
            categoryService.CategoryServiceId = ObjectId.GenerateNewId().ToString();
            categoryService.AccId = "685660321fc7aebe254c4be1";
            categoryService.CreateAt = DateTime.UtcNow;
            categoryService.UpdateAt = null;
            categoryService.IsDeleted = false;

            await _CategoryServices.InsertOneAsync(categoryService);
            return categoryService;
        }

        public async Task<CategoryService> UpdateAsync(string categoryServiceId, CategoryService updateCategory)
        {
            if (!ObjectId.TryParse(categoryServiceId, out _)) return null;

            var filter = Builders<CategoryService>.Filter.Eq(g => g.CategoryServiceId, categoryServiceId);

            if (filter == null) return null;

            var update = Builders<CategoryService>.Update
                .Set(g => g.CategoryName, updateCategory.CategoryName)
                .Set(g => g.CategoryDescription, updateCategory.CategoryDescription)
                .Set(g => g.UpdateAt, DateTime.UtcNow);

            var result = await _CategoryServices.UpdateOneAsync(filter, update);

            var updatedCategory = await _CategoryServices.Find(g => g.CategoryServiceId == categoryServiceId && g.IsDeleted != true).FirstOrDefaultAsync();

            return updatedCategory;
        }

        public async Task<long> DeleteAsync(string categoryServiceId)
        {
            if (!ObjectId.TryParse(categoryServiceId, out _)) return 0;

            var filter = Builders<CategoryService>.Filter.Eq(g => g.CategoryServiceId, categoryServiceId);

            var update = Builders<CategoryService>.Update
                        .Set(g => g.IsDeleted, true);

            var result = await _CategoryServices.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }
        public async Task<long> Restore(string categoryServiceId)
        {
            if (!ObjectId.TryParse(categoryServiceId, out _)) return 0;

            var filter = Builders<CategoryService>.Filter.Eq(g => g.CategoryServiceId, categoryServiceId);

            if (filter == null) return 0;

            var update = Builders<CategoryService>.Update
                .Set(g => g.IsDeleted, false)
                .Set(g => g.UpdateAt, DateTime.UtcNow);

            var result = await _CategoryServices.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }
    }
}
