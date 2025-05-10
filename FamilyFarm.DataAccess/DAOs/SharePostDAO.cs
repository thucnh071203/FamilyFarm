using FamilyFarm.Models.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class SharePostDAO
    {
        private readonly IMongoCollection<SharePost> _sharePosts;

        public SharePostDAO(IMongoDatabase database)
        {
            _sharePosts = database.GetCollection<SharePost>("SharePost");
        }

        public async Task<SharePost?> GetById(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId))
                return null;

            var filter = Builders<SharePost>.Filter.Eq(x => x.SharePostId, sharePostId);

            var post = await _sharePosts.Find(filter).FirstOrDefaultAsync();
            return post;
        }

        public async Task<List<SharePost>?> GetByAccId(string? accId)
        {
            if (string.IsNullOrEmpty(accId))
                return null;

            var filter = Builders<SharePost>.Filter.Eq(x => x.AccId, accId);

            var posts = await _sharePosts.Find(filter).ToListAsync();
            return posts;
        }

        public async Task<SharePost?> CreateAsync(SharePost? request)
        {
            if (request == null)
                return null;

            request.PostId = ObjectId.GenerateNewId().ToString();
            request.CreatedAt = DateTime.UtcNow;
            await _sharePosts.InsertOneAsync(request);

            return request;
        }

        public async Task<SharePost?> UpdateAsync(SharePost? request)
        {
            if (request == null || string.IsNullOrEmpty(request.PostId))
            {
                return null;
            }

                var filter = Builders<SharePost>.Filter.Eq(x => x.SharePostId, request.SharePostId);

                var update = Builders<SharePost>.Update
                    .Set(x => x.SharePostContent, request.SharePostContent)
                    .Set(x => x.SharePostScope, request.SharePostScope)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);
                // Thêm các field khác bạn muốn update ở đây

                var result = await _sharePosts.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    // Sau khi update, lấy lại Post mới nhất để trả về
                    var updatedPost = await _sharePosts.Find(filter).FirstOrDefaultAsync();
                    return updatedPost;
                }
                else
                {
                    return null;
                }
        }

        public async Task<bool> HardDeleteAsync(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId)) return false;

            var filter = Builders<SharePost>.Filter.Eq(p => p.SharePostId, sharePostId);
            var result = await _sharePosts.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> SoftDeleteAsync(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId)) return false;

            var filter = Builders<SharePost>.Filter.Eq(p => p.SharePostId, sharePostId);
            var update = Builders<SharePost>.Update.Set(p => p.IsDeleted, true)
                                                .Set(p => p.DeletedAt, DateTime.UtcNow);

            var result = await _sharePosts.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}
