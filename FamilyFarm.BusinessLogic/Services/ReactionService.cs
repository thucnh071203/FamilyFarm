using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
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

        public async Task<bool> ToggleReactionAsync(string entityId, string entityType, string accId, string categoryReactionId)
        {
            // Validate ObjectId inputs
            if (!ObjectId.TryParse(entityId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
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

        public async Task<ListReactionResponseDTO> GetAllByEntityAsync(string entityId, string entityType)
        {
            // Validate ObjectId input
            if (!ObjectId.TryParse(entityId, out _))
                return new ListReactionResponseDTO
                {
                    Success = false,
                    Message = "No reaction found!.",
                    AvailableCount = 0,
                    Reactions = new List<Reaction>()
                };

            var reactions = await _reactionRepository.GetAllByEntityAsync(entityId, entityType);

            return new ListReactionResponseDTO
            {
                Success = true,
                Message = "Get list of reactions successfully!",
                AvailableCount = reactions.Where(r => r.IsDeleted != true).Count(),
                Reactions = reactions
            };
        }
    }
}