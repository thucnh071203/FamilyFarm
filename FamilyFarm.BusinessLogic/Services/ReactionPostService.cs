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
    public class ReactionPostService : IReactionPostService
    {
        private readonly IReactionPostRepository _reactionPostRepository;
        private readonly ICategoryReactionRepository _categoryReactionRepository;

        public ReactionPostService(IReactionPostRepository reactionPostRepository, ICategoryReactionRepository categoryReactionRepository)
        {
            _reactionPostRepository = reactionPostRepository;
            _categoryReactionRepository = categoryReactionRepository;
        }

        /// <summary>
        /// Toggle reaction for a post, only applies to default reactions.
        /// If reaction already exists: toggle IsDeleted status.
        /// If reaction does not exist: return false (do not create new one).
        /// </summary>
        /// <param name="postId">ID of post</param>
        /// <param name="categoryReactionId">ID of reaction</param>
        /// <returns>True if toggle succeeds, False if fails</returns>
        public async Task<bool> ToggleReactionAsync(string postId, string accId, string categoryReactionId)
        {
            // Check ID validity
            if (!ObjectId.TryParse(postId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
                return false;

            // Check if CategoryReaction exists and is not soft deleted
            var categoryReaction = await _categoryReactionRepository.GetByIdAsync(categoryReactionId);
            if (categoryReaction == null || categoryReaction.IsDeleted == true)
                return false;

            // Check if the user has any reactions to the post
            var existingReaction = await _reactionPostRepository.GetByPostAndAccAsync(postId, accId);

            if (existingReaction == null)
            {
                // Create new reaction if it doesn't exist yet
                var newReaction = new ReactionPost
                {
                    ReactPostId = ObjectId.GenerateNewId().ToString(),
                    PostId = postId,
                    AccId = accId,
                    CategoryReactionId = categoryReactionId,
                    IsDeleted = false
                };
                await _reactionPostRepository.CreateAsync(newReaction);
                return true;
            }
            else
            {
                // If select same reaction, toggle IsDeleted status
                if (existingReaction.CategoryReactionId == categoryReactionId)
                {
                    if (existingReaction.IsDeleted == true)
                        return await _reactionPostRepository.RestoreAsync(existingReaction.ReactPostId);
                    else
                        return await _reactionPostRepository.DeleteAsync(existingReaction.ReactPostId);
                }
                else
                {
                    // If another reaction is selected, update CategoryReactionId and set IsDeleted = false
                    return await _reactionPostRepository.UpdateAsync(
                        existingReaction.ReactPostId,
                        categoryReactionId,
                        false
                    );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<List<ReactionPost>> GetAllByPostAsync(string postId)
        {
            return await _reactionPostRepository.GetAllByPostAsync(postId);
        }

    }
}
