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
    public class PostTagDAO
    {
        private readonly IMongoCollection<PostTag> _postTagCollection;

        public PostTagDAO(IMongoDatabase database)
        {
            _postTagCollection = database.GetCollection<PostTag>("PostTag");
        }

        /// <summary>
        ///     Create new post tag
        /// </summary>
        public async Task<PostTag?> CreatePostTag(PostTag? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            request.CreatedAt = DateTime.UtcNow;

            await _postTagCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list post tag of post id
        /// </summary>
        public async Task<List<PostTag>?> GetAllPostTagOfPost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return null;

            var result = await _postTagCollection
                .Find(pc => pc.PostId == post_id)
                .ToListAsync();

            return result;
        }

        /// <summary>
        ///     Delete post tag by id
        /// </summary>
        public async Task<bool> DeleteTagById(string? post_tag_id)
        {
            if (string.IsNullOrEmpty(post_tag_id))
                return false;

            try
            {
                var filter = Builders<PostTag>.Filter.Eq(x => x.PostTagId, post_tag_id);
                var result = await _postTagCollection.DeleteOneAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        ///     Delete all post tag by post id
        /// </summary>
        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return false;

            try
            {
                var filter = Builders<PostTag>.Filter.Eq(x => x.PostId, post_id);
                var result = await _postTagCollection.DeleteManyAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
