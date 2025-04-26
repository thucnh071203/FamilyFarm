using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class CategoryPostDAO
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
                .Find(c => c.CategoryId == category_id)
                .FirstOrDefaultAsync();

            return category;
        }
    }
}
