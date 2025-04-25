using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ReactionPostDAO
    {
        private readonly IMongoCollection<ReactionPost> _reactionPosts;

        public ReactionPostDAO(IMongoDatabase database)
        {
            _reactionPosts = database.GetCollection<ReactionPost>("ReactionPost");
        }

        /// <summary>
        /// Get ReactionPost information by PostId, AccId and CategoryReactionId.
        /// Used to check if the user has reacted to the post with any type of reaction.
        /// </summary>
        /// <param name="postId">ID of the post.</param>
        /// <param name="accId">ID of the user.</param>
        /// <param name="categoryReactionId">ID of the reaction type.</param>
        /// <returns>ReactionPost object if it exists, otherwise null.</returns>
        public async Task<ReactionPost> GetByPostAccAndReactionAsync(string postId, string accId, string categoryReactionId)
        {
            if (!ObjectId.TryParse(postId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
                return null;
            return await _reactionPosts
                .Find(r => r.PostId == postId && r.AccId == accId && r.CategoryReactionId == categoryReactionId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get the user's reaction to a post (regardless of reaction type).
        /// </summary>
        /// <param name="postId">Post ID.</param>
        /// <param name="accId">User ID.</param>
        /// <returns>ReactionPost if exists, otherwise null.</returns>
        public async Task<ReactionPost> GetByPostAndAccAsync(string postId, string accId)
        {
            if (!ObjectId.TryParse(postId, out _) || !ObjectId.TryParse(accId, out _))
                return null;
            return await _reactionPosts
                .Find(r => r.PostId == postId && r.AccId == accId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all reactions (not deleted) of a post.
        /// </summary>
        /// <param name="postId">Post ID.</param>
        /// <returns>List of ReactionPosts of the post.</returns>
        public async Task<List<ReactionPost>> GetAllByPostAsync(string postId)
        {
            if (!ObjectId.TryParse(postId, out _))
                return new List<ReactionPost>();
            return await _reactionPosts
                .Find(r => r.PostId == postId && r.IsDeleted != true)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new reaction for the post.
        /// </summary>
        /// <param name="reactionPost">The ReactionPost object to create.</param>
        /// <returns>The ReactionPost object after successful creation.</returns>
        public async Task<ReactionPost> CreateAsync(ReactionPost reactionPost)
        {
            reactionPost.ReactPostId = ObjectId.GenerateNewId().ToString();
            reactionPost.CreateAt = DateTime.UtcNow;
            reactionPost.UpdateAt = DateTime.UtcNow;
            reactionPost.IsDeleted = false;
            await _reactionPosts.InsertOneAsync(reactionPost);
            return reactionPost;
        }

        /// <summary>
        /// Update the reaction type and delete status for a specific reaction.
        /// </summary>
        /// <param name="reactPostId">The reaction ID to update.</param>
        /// <param name="categoryReactionId">The new reaction type ID.</param>
        /// <param name="isDeleted">The new delete status.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public async Task<bool> UpdateAsync(string reactPostId, string categoryReactionId, bool isDeleted)
        {
            var filter = Builders<ReactionPost>.Filter.Where(r => r.ReactPostId == reactPostId);
            var update = Builders<ReactionPost>.Update
                .Set(r => r.CategoryReactionId, categoryReactionId)
                .Set(r => r.IsDeleted, isDeleted)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactionPosts.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Soft delete a reaction by setting IsDeleted = true.
        /// </summary>
        /// <param name="reactPostId">The reaction ID to delete.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteAsync(string reactPostId)
        {
            var filter = Builders<ReactionPost>.Filter.Where(r => r.ReactPostId == reactPostId);
            var update = Builders<ReactionPost>.Update
                .Set(r => r.IsDeleted, true)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactionPosts.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Restore a soft deleted reaction by setting IsDeleted = false.
        /// </summary>
        /// <param name="reactPostId">The reaction ID to restore.</param>
        /// <returns>True if the restore was successful, otherwise false.</returns>
        public async Task<bool> RestoreAsync(string reactPostId)
        {
            var filter = Builders<ReactionPost>.Filter.Where(r => r.ReactPostId == reactPostId);
            var update = Builders<ReactionPost>.Update
                .Set(r => r.IsDeleted, false)
                .Set(r => r.UpdateAt, DateTime.UtcNow);

            var result = await _reactionPosts.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
