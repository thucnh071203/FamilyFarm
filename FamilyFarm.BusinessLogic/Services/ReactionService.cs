using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ReactionService : IReactionService
    {
        private readonly IReactionRepository _reactionRepository;
        private readonly ICategoryReactionRepository _categoryReactionRepository;

        public ReactionService(IReactionRepository reactionRepository, ICategoryReactionRepository categoryReactionRepository)
        {
            _reactionRepository = reactionRepository;
            _categoryReactionRepository = categoryReactionRepository;
        }

        /// <summary>
        /// Toggle reaction for an entity (Post or Comment), only applies to default reactions.
        /// If reaction already exists: toggle IsDeleted status.
        /// If reaction does not exist: create new one.
        /// </summary>
        /// <param name="entityId">ID of entity (Post or Comment)</param>
        /// <param name="entityType">Type of entity ("Post" or "Comment")</param>
        /// <param name="accId">ID of account</param>
        /// <param name="categoryReactionId">ID of reaction</param>
        /// <returns>True if toggle succeeds, False if fails</returns>
        public async Task<bool> ToggleReactionAsync(string entityId, string entityType, string accId, string categoryReactionId)
        {
            // Validate inputs
            if (!ObjectId.TryParse(entityId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
                return false;

            if (entityType != "Post" && entityType != "Comment")
                return false;

            // Check if CategoryReaction exists and is not soft deleted
            var categoryReaction = await _categoryReactionRepository.GetByIdAsync(categoryReactionId);
            if (categoryReaction == null || categoryReaction.IsDeleted == true)
                return false;

            // Check if the user has any reactions to the entity
            var existingReaction = await _reactionRepository.GetByEntityAndAccAsync(entityId, entityType, accId);

            if (existingReaction == null)
            {
                // Create new reaction if it doesn't exist yet
                var newReaction = new Reaction
                {
                    ReactionId = ObjectId.GenerateNewId().ToString(),
                    EntityId = entityId,
                    EntityType = entityType,
                    AccId = accId,
                    CategoryReactionId = categoryReactionId,
                    IsDeleted = false
                };
                await _reactionRepository.CreateAsync(newReaction);
                return true;
            }
            else
            {
                // If select same reaction, toggle IsDeleted status
                if (existingReaction.CategoryReactionId == categoryReactionId)
                {
                    if (existingReaction.IsDeleted == true)
                        return await _reactionRepository.RestoreAsync(existingReaction.ReactionId);
                    else
                        return await _reactionRepository.DeleteAsync(existingReaction.ReactionId);
                }
                else
                {
                    // If another reaction is selected, update CategoryReactionId and set IsDeleted = false
                    return await _reactionRepository.UpdateAsync(
                        existingReaction.ReactionId,
                        categoryReactionId,
                        false
                    );
                }
            }
        }

        /// <summary>
        /// Get all reactions for an entity (Post or Comment).
        /// </summary>
        /// <param name="entityId">ID of entity</param>
        /// <param name="entityType">Type of entity ("Post" or "Comment")</param>
        /// <returns>List of reactions</returns>
        public async Task<List<Reaction>> GetAllByEntityAsync(string entityId, string entityType)
        {
            return await _reactionRepository.GetAllByEntityAsync(entityId, entityType);
        }
    }
}
