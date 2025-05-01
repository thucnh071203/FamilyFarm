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
    public class CategoryPostRepository : ICategoryPostRepository
    {
        private readonly CategoryPostDAO _dao;

        public CategoryPostRepository(CategoryPostDAO dao)
        {
            _dao = dao;
        }

        public async Task<Category?> GetCategoryById(string? category_id)
        {
            return await _dao.GetCategoryById(category_id);
        }
        public async Task<List<Category>?> GetListCategory()
        {
            return await _dao.GetListCategory();
        }
        public async Task<bool> Delete(string categoryId)
        {
            return await _dao.Delete(categoryId);
        }
        public async Task<bool> Update(Category category)
        {
            return await _dao.Update(category);
        }
        public async Task<Category?> Create(Category category)
        {
            return await _dao.Create(category);
        }
    }
}
