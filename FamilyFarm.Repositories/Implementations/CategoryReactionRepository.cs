﻿using FamilyFarm.DataAccess.DAOs;
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

        public Task CreateAsync(CategoryReaction reaction) => _categoryReactionDAO.CreateAsync(reaction);

        public Task<bool> UpdateAsync(string id, CategoryReaction reaction) => _categoryReactionDAO.UpdateAsync(id, reaction);

        public Task<bool> DeleteAsync(string id) => _categoryReactionDAO.SoftDeleteAsync(id);
    }
}
