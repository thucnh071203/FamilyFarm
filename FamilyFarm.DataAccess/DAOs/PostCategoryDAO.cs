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
    public class PostCategoryDAO
    {
        private readonly IMongoCollection<PostCategory> _postCategoryCollection;

        public PostCategoryDAO(IMongoDatabase database)
        {
            _postCategoryCollection = database.GetCollection<PostCategory>("PostCategory");
        }

        /// <summary>
        ///     Create new post category
        /// </summary>
        public async Task<PostCategory?> CreatePostCategory(PostCategory? request)
        {
            if (request == null) 
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            request.CreatedAt = DateTime.UtcNow;

            await _postCategoryCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list category of post id
        /// </summary>
        public async Task<List<PostCategory>?> GetAllCategoryOfPost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id)) 
                return null;

            var categoriesOfPost = await _postCategoryCollection
                .Find(pc => pc.PostId == post_id)
                .ToListAsync();

            return categoriesOfPost;
        }
    }
}
