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

        /// <summary>
        /// Retrieves a category notification by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the category notification to retrieve.</param>
        /// <returns>
        /// The matching <see cref="CategoryNotification"/> if found; otherwise, null.
        /// </returns>
        public async Task<CategoryNotification?> GetByIdAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var category = await _categoryNotification
                .Find(c => c.CategoryNotifiId == id)
                .FirstOrDefaultAsync();

            return category;
        }

        /// <summary>
        /// Retrieves a category notification by its name (case-insensitive).
        /// </summary>
        /// <param name="name">The name of the category notification to retrieve.</param>
        /// <returns>
        /// The matching <see cref="CategoryNotification"/> if found; otherwise, null.
        /// </returns>
        public async Task<CategoryNotification?> GetByNameAsync(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var category = await _categoryNotification
                .Find(c => c.CategoryNotifiName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();

            return category;
        }
    }
}
