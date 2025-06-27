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
    public class CategoryReactionRepository : ICategoryReactionRepository
    {
        private readonly CategoryReactionDAO _categoryReactionDAO;

        public CategoryReactionRepository(CategoryReactionDAO categoryReactionDAO)
        {
            _categoryReactionDAO = categoryReactionDAO;
        }

        public async Task<List<CategoryReaction>> GetAllAsync()
        {
            return await _categoryReactionDAO.GetAllAsync();
        }

        public async Task<CategoryReaction> GetByIdAsync(string id)
        {
            return await _categoryReactionDAO.GetByIdAsync(id);
        }

        public async Task CreateAsync(CategoryReaction reaction) => await _categoryReactionDAO.CreateAsync(reaction);

        public async Task<bool> UpdateAsync(string id, CategoryReaction reaction) => await _categoryReactionDAO.UpdateAsync(id, reaction);

        public async Task<bool> DeleteAsync(string id) => await _categoryReactionDAO.SoftDeleteAsync(id);
        public async Task<bool> RestoreAsync(string id) => await _categoryReactionDAO.RestoreAsync(id);
    }
}
