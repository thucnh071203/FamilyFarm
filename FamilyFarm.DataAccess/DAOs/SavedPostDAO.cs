using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class SavedPostDAO
    {
        private readonly IMongoCollection<SavedPost> _savedPostCollection;

        public SavedPostDAO(IMongoDatabase database)
        {
            _savedPostCollection = database.GetCollection<SavedPost>("SavedPost");
        }


        /// <summary>
        ///     Create new saved post
        /// </summary>
        public async Task<SavedPost?> CreateAsync(SavedPost? request)
        {
            if(request == null) 
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.SavedPostId))
            {
                request.SavedPostId = ObjectId.GenerateNewId().ToString();
            }

            request.SavedAt = DateTime.UtcNow;
            request.IsDeleted = false;

            await _savedPostCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list all saved post of account
        /// </summary>
        public async Task<List<SavedPost>?> GetListAvailableByAccount(string? accId)
        {
            if (string.IsNullOrEmpty(accId))
                return null;

            var filter = Builders<SavedPost>.Filter.And(
                Builders<SavedPost>.Filter.Eq(x => x.AccId, accId),
                Builders<SavedPost>.Filter.Eq(x => x.IsDeleted, false)
            );

            var savedPosts = await _savedPostCollection.Find(filter).ToListAsync();

            return savedPosts;
        }

        public async Task<bool?> IsSavedPost(string? accId, string? postId)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(accId))
                return null;

            var filter = Builders<SavedPost>.Filter.Eq(sp => sp.PostId, postId) &
                         Builders<SavedPost>.Filter.Eq(sp => sp.AccId, accId);

            var exists = await _savedPostCollection.Find(filter).AnyAsync();
            return exists;
        }

        public async Task<bool?> DeleteSavedPost(string? accId, string? postId)
        {
            if (string.IsNullOrEmpty(accId) || string.IsNullOrEmpty(postId))
                return null;

            var filter = Builders<SavedPost>.Filter.Eq(sp => sp.AccId, accId) &
                         Builders<SavedPost>.Filter.Eq(sp => sp.PostId, postId);

            var result = await _savedPostCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }
    }
}
