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
    }
}
