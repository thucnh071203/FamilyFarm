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
    }
}
