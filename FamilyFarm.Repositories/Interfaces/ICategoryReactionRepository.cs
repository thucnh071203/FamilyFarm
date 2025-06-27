using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ICategoryReactionRepository
    {
        Task<List<CategoryReaction>> GetAllAsync();
        Task<CategoryReaction> GetByIdAsync(string id);
        Task CreateAsync(CategoryReaction reaction);
        Task<bool> UpdateAsync(string id, CategoryReaction reaction);
        Task<bool> DeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
    }
}
