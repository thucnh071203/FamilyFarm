using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CategoryReactionService : ICategoryReactionService
    {
        private readonly ICategoryReactionRepository _categoryReactionRepository;

        public CategoryReactionService(ICategoryReactionRepository categoryReactionRepository)
        {
            _categoryReactionRepository = categoryReactionRepository;
        }

        /// <summary>
        /// Retrieves a CategoryReaction by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the CategoryReaction.</param>
        /// <returns>The CategoryReaction if found, null otherwise.</returns>
        public async Task<CategoryReaction> GetByIdAsync(string id)
        {
            return await _categoryReactionRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Retrieves all category reactions from the data source, including those marked as deleted.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of all CategoryReaction objects.
        /// </returns>
        public async Task<List<CategoryReaction>> GetAllAsync()
        {
            return await _categoryReactionRepository.GetAllAsync();
        }

        /// <summary>
        /// Retrieves all available (non-deleted) category reactions from the database.
        /// </summary>
        /// <returns>
        /// A list of CategoryReaction objects where the IsDeleted flag is not true.
        /// </returns>
        public async Task<List<CategoryReaction>> GetAllAvalableAsync()
        {
            var categoryReactions = await _categoryReactionRepository.GetAllAsync();
            var result = categoryReactions.Where(r => r.IsDeleted != true).ToList();
            return result;
        }


        public async Task CreateAsync(CategoryReaction reaction) => await _categoryReactionRepository.CreateAsync(reaction);

        public async Task<bool> UpdateAsync(string id, CategoryReaction reaction) => await _categoryReactionRepository.UpdateAsync(id, reaction);

        public async Task<bool> DeleteAsync(string id) => await _categoryReactionRepository.DeleteAsync(id);

        public async Task<bool> RestoreAsync(string id) => await _categoryReactionRepository.RestoreAsync(id);
    }
}

