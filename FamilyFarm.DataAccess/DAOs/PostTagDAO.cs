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
    }
}
