using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class CategoryServiceRepository : ICategoryServiceRepository
    {
        private readonly CategoryServiceDAO _dao;
        public CategoryServiceRepository(CategoryServiceDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<CategoryService>> GetAllCategoryService()
        {
            return await _dao.GetAllAsync();
        }
        public async Task<List<CategoryService>> GetAllForAdmin()
        {
            return await _dao.GetAllForAdmin();
        }

        public async Task<CategoryService> GetCategoryServiceById(string categoryServiceId)
        {
            return await _dao.GetByIdAsync(categoryServiceId);
        }

        public async Task<CategoryService> CreateCategoryService(CategoryService item)
        {
            return await _dao.CreateAsync(item);
        }

        public async Task<CategoryService> UpdateCategoryService(string categoryServiceId, CategoryService item)
        {
            return await _dao.UpdateAsync(categoryServiceId, item);
        }

        public async Task<long> DeleteCategoryService(string categoryServiceId)
        {
            return await _dao.DeleteAsync(categoryServiceId);
        }
        public async Task<long> Restore(string categoryServiceId)
        {
            return await _dao.Restore(categoryServiceId);
        }
    }
}
