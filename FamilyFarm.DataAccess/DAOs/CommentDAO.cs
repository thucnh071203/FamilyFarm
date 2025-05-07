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
    public class CommentDAO
    {
        private readonly IMongoCollection<Comment> _Comments;
        public CommentDAO(IMongoDatabase database)
        {
            _Comments = database.GetCollection<Comment>("Comment");
        }

        /// <summary>
        /// Retrieves all comments associated with a specific post ID.
        /// Filters out comments that are marked as deleted.
        /// </summary>
        /// <param name="postId">The ID of the post to fetch comments for</param>
        /// <returns>List of non-deleted comments for the given post</returns>
        public async Task<List<Comment>> GetAllByPostAsync(string postId)
        {
            if (!ObjectId.TryParse(postId, out _)) return new List<Comment>();
            return await _Comments.Find(c => c.PostId == postId && c.IsDeleted == false).ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific comment by its ID, only if it is not marked as deleted.
        /// </summary>
        /// <param name="id">The ID of the comment</param>
        /// <returns>The comment if found and not deleted; otherwise, null</returns>
        public async Task<Comment?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) 
                return null;
            return await _Comments.Find(c => c.CommentId == id && c.IsDeleted != true).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new comment with a generated ObjectId and current UTC timestamp.
        /// </summary>
        /// <param name="comment">The comment object to insert</param>
        /// <returns>The inserted comment with assigned ID and timestamp</returns>
        public async Task<Comment> CreateAsync(Comment comment)
        {
            if (!ObjectId.TryParse(comment.PostId, out _) || !ObjectId.TryParse(comment.AccId, out _))
                return null;
            comment.CommentId = ObjectId.GenerateNewId().ToString();
            comment.CreateAt = DateTime.UtcNow;
            await _Comments.InsertOneAsync(comment);
            return comment;
        }

        /// <summary>
        /// Updates an existing comment by ID.
        /// Replaces the entire comment document and updates the timestamp.
        /// </summary>
        /// <param name="id">The ID of the comment to update</param>
        /// <param name="comment">The updated comment data</param>
        /// <returns>The updated comment if successful; otherwise, null</returns>
        public async Task<Comment> UpdateAsync(string id, Comment comment)
        {
            var existing = await _Comments.Find(c => c.CommentId == id && c.IsDeleted != true).FirstOrDefaultAsync();
            if (existing == null) return null;

            comment.CommentId = id;
            comment.CreateAt = DateTime.UtcNow;
            await _Comments.ReplaceOneAsync(c => c.CommentId == id && c.IsDeleted != true, comment);
            return comment;
        }

        /// <summary>
        /// Performs a soft delete on a comment by setting IsDeleted = true.
        /// </summary>
        /// <param name="id">The ID of the comment to delete</param>
        /// <returns>True if deletion was successful; otherwise, false</returns>
        public async Task DeleteAsync(string id)
        {
            var filter = Builders<Comment>.Filter.Where(c => c.CommentId == id && c.IsDeleted != true);
            var update = Builders<Comment>.Update.Set(c => c.IsDeleted, true);

            await _Comments.UpdateOneAsync(filter, update);
        }

    }
}
