using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FamilyFarm.DataAccess.DAOs
{
    public class CategoryNotificationDAO : SingletonBase
    {
        private readonly IMongoCollection<CategoryNotification> _categoryNotification;

        public CategoryNotificationDAO(IMongoDatabase database)
        {
            _categoryNotification = database.GetCollection<CategoryNotification>("CategoryNotification");
        }

        public async Task<CategoryNotification?> GetByIdAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var category = await _categoryNotification
                .Find(c => c.CategoryNotifiId == id)
                .FirstOrDefaultAsync();

            return category;
        }
        
        public async Task<CategoryNotification?> GetByNameAsync(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var category = await _categoryNotification
                .Find(c => c.CategoryNotifiName == name)
                .FirstOrDefaultAsync();

            return category;
        }
    }
}
