using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ICategoryReactionService
    {
        Task<List<CategoryReaction>> GetAllAsync();
        Task<List<CategoryReaction>> GetAllAvalableAsync();
        Task<CategoryReaction> GetByIdAsync(string id);
        Task CreateAsync(CategoryReaction reaction);
        Task<bool> UpdateAsync(string id, CategoryReaction reaction);
        Task<bool> DeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
    }
}
