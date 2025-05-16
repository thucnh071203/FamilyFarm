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
    public class ReactionRepository : IReactionRepository
    {
        private readonly ReactionDAO _reactionDAO;

        public ReactionRepository(ReactionDAO reactionDAO)
        {
            _reactionDAO = reactionDAO;
        }

        public async Task<Reaction> GetByEntityAccAndReactionAsync(string entityId, string accId, string entityType, string categoryReactionId)
        {
            return await _reactionDAO.GetByEntityAccAndReactionAsync(entityId, entityType, accId, categoryReactionId);
        }

        public async Task<Reaction> GetByEntityAndAccAsync(string entityId, string entityType, string accId)
        {
            return await _reactionDAO.GetByEntityAndAccAsync(entityId, entityType, accId);
        }

        public async Task<List<Reaction>> GetAllByEntityAsync(string entityId, string entityType)
        {
            return await _reactionDAO.GetAllByEntityAsync(entityId, entityType);
        }

        public async Task<Reaction> CreateAsync(Reaction reaction)
        {

            return await _reactionDAO.CreateAsync(reaction);
        }

        public async Task<bool> UpdateAsync(string reactionId, string categoryReactionId, bool isDeleted)
        {
            return await _reactionDAO.UpdateAsync(reactionId, categoryReactionId, isDeleted);
        }

        public async Task<bool> DeleteAsync(string reactionId)
        {
            return await _reactionDAO.DeleteAsync(reactionId);
        }

        public async Task<bool> RestoreAsync(string reactionId)
        {
            return await _reactionDAO.RestoreAsync(reactionId);
        }
    }
}
