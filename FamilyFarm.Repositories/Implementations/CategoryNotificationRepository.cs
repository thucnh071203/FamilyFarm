using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class CategoryNotificationRepository : ICategoryNotificationRepository
    {
        private readonly CategoryNotificationDAO _categoryNotificationDAO;

        public CategoryNotificationRepository(CategoryNotificationDAO categoryNotificationDAO)
        {
            _categoryNotificationDAO = categoryNotificationDAO;
        }

        public async Task<CategoryNotification?> GetByIdAsync(string? id)
        {
            return await _categoryNotificationDAO.GetByIdAsync(id);
        }

        public async Task<CategoryNotification?> GetByNameAsync(string? name)
        {
            return await _categoryNotificationDAO.GetByNameAsync(name);
        }
    }
}
