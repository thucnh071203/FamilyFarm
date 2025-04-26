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
    public class PostImageDAO
    {
        private readonly IMongoCollection<PostImage> _postImageCollection;

        public PostImageDAO(IMongoDatabase database)
        {
            _postImageCollection = database.GetCollection<PostImage>("PostImage");
        }


        /// <summary>
        ///     Create new post image
        /// </summary>
        public async Task<PostImage?> CreatePostImage(PostImage? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            await _postImageCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list category of post id
        /// </summary>
        public async Task<List<PostImage>?> GetAllImageOfPost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return null;

            var imagesOfPost = await _postImageCollection
                .Find(pc => pc.PostId == post_id)
                .ToListAsync();

            return imagesOfPost;
        }
    }
}
