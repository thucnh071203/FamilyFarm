using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class HashtagDAO
    {
        private readonly IMongoCollection<HashTag> _hashtagCollection;

        public HashtagDAO(IMongoDatabase database)
        {
            _hashtagCollection = database.GetCollection<HashTag>("HashTag");
        }

        /// <summary>
        ///     Create hash tag
        /// </summary>
        public async Task<HashTag?> CreateHashTag(HashTag? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            await _hashtagCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list hashtag of post id
        /// </summary>
        public async Task<List<HashTag>?> GetAllHashTagOfPost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return null;

            var result = await _hashtagCollection
                .Find(pc => pc.PostId == post_id)
                .ToListAsync();

            return result;
        }

        /// <summary>
        ///     Delete hashtag by hashtag id
        /// </summary>
        public async Task<bool> DeleteHashTagById(string? hashtag_id)
        {
            if (string.IsNullOrEmpty(hashtag_id))
                return false;

            try
            {
                var filter = Builders<HashTag>.Filter.Eq(x => x.HashTagId, hashtag_id);
                var result = await _hashtagCollection.DeleteOneAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Delete all hashtag by post id
        /// </summary>
        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return false;

            try
            {
                var filter = Builders<HashTag>.Filter.Eq(x => x.PostId, post_id);
                var result = await _hashtagCollection.DeleteManyAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Update value field IsDeleted of all hashtag of post id = TRUE
        /// </summary>
        public async Task<bool> InactiveAllByPostId(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return false;

            try
            {
                var filter = Builders<HashTag>.Filter.Eq(x => x.PostId, post_id);
                var update = Builders<HashTag>.Update.Set(x => x.IsDeleted, true);

                var result = await _hashtagCollection.UpdateManyAsync(filter, update);

                return result.ModifiedCount > 0;

            } catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        ///     Update value field IsDeleted of all hashtag of post id = FALSE
        /// </summary>
        public async Task<bool> ActiveAllByPostId(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return false;

            try
            {
                var filter = Builders<HashTag>.Filter.Eq(x => x.PostId, post_id);
                var update = Builders<HashTag>.Update.Set(x => x.IsDeleted, false);

                var result = await _hashtagCollection.UpdateManyAsync(filter, update);

                return result.ModifiedCount > 0;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
