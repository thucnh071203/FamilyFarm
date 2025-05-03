using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IReactionRepository
    {
        Task<Reaction> GetByEntityAccAndReactionAsync(string entityId, string accId, string entityType, string categoryReactionId);
        Task<Reaction> GetByEntityAndAccAsync(string entityId, string entityType, string accId);
        Task<List<Reaction>> GetAllByEntityAsync(string entityId, string entityType);
        Task<Reaction> CreateAsync(Reaction reaction);
        Task<bool> UpdateAsync(string reactionId, string categoryReactionId, bool isDeleted);
        Task<bool> DeleteAsync(string reactionId);
        Task<bool> RestoreAsync(string reactionId);
    }
}
