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

        public async Task<PostImage?> GetById(string? image_id)
        {
            if (string.IsNullOrEmpty(image_id))
            {
                return null;
            }

            var filter = Builders<PostImage>.Filter.Eq(x => x.PostImageId, image_id);
            return await _postImageCollection.Find(filter).FirstOrDefaultAsync();
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

        /// <summary>
        ///     Delete post image by id
        /// </summary>
        public async Task<bool> DeleteImageById(string? image_id)
        {
            if (string.IsNullOrEmpty(image_id))
                return false;

            try
            {
                var filter = Builders<PostImage>.Filter.Eq(x => x.PostImageId, image_id);
                var result = await _postImageCollection.DeleteOneAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        ///     Delete all post image by post id
        /// </summary>
        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            if (!string.IsNullOrEmpty(post_id))
                return false;

            try
            {
                var filter = Builders<PostImage>.Filter.Eq(x => x.PostId, post_id);
                var result = await _postImageCollection.DeleteManyAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
