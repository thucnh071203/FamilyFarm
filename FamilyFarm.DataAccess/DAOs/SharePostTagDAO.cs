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
    public class SharePostTagDAO
    {
        private readonly IMongoCollection<SharePostTag> _sharePostTags;

        public SharePostTagDAO(IMongoDatabase database)
        {
            _sharePostTags = database.GetCollection<SharePostTag>("SharePostTag");
        }

        /// <summary>
        ///     Create new post tag
        /// </summary>
        public async Task<SharePostTag?> CreateAsyns(SharePostTag? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.SharePostId))
            {
                request.SharePostId = ObjectId.GenerateNewId().ToString();
            }

            request.CreatedAt = DateTime.UtcNow;

            await _sharePostTags.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list post tag of post id
        /// </summary>
        public async Task<List<SharePostTag>?> GetAllBySharePost(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId))
                return null;

            var result = await _sharePostTags
                .Find(pc => pc.SharePostId == sharePostId)
                .ToListAsync();

            return result;
        }

        /// <summary>
        ///     Delete post tag by id
        /// </summary>
        public async Task<bool> DeleteTagById(string? sharePostTagId)
        {
            if (string.IsNullOrEmpty(sharePostTagId))
                return false;

            try
            {
                var filter = Builders<SharePostTag>.Filter.Eq(x => x.SharePostTagId, sharePostTagId);
                var result = await _sharePostTags.DeleteOneAsync(filter);

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
        public async Task<bool> DeleteAllBySharePostId(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId))
                return false;

            try
            {
                var filter = Builders<SharePostTag>.Filter.Eq(x => x.SharePostId, sharePostId);
                var result = await _sharePostTags.DeleteManyAsync(filter);

                return result.DeletedCount > 0; // true nếu xóa được ít nhất 1 document
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
