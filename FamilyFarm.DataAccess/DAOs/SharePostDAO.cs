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

        public async Task<List<SharePost>> GetListInfiniteSharePost(string? lastSharePostId, int pageSize)
        {
            var filterBuilder = Builders<SharePost>.Filter;

            // Điều kiện lọc theo isDeleted = false
            var isDeletedFilter = filterBuilder.Eq(sp => sp.IsDeleted, false);
            // Điều kiện SharePostScope = "Public"
            var scopeFilter = filterBuilder.Eq(sp => sp.SharePostScope, "Public");
            // Điều kiện Status = 0
            var scopeStatus = filterBuilder.Eq(sp => sp.Status, 0);

            // Kết hợp các filter chung
            var filters = new List<FilterDefinition<SharePost>> { isDeletedFilter, scopeFilter, scopeStatus };

            if (!string.IsNullOrEmpty(lastSharePostId))
            {
                // Thêm điều kiện phân trang: SharePostId < lastSharePostId
                var lastIdFilter = filterBuilder.Lt(sp => sp.SharePostId, lastSharePostId);
                filters.Add(lastIdFilter);
            }

            // Kết hợp tất cả các filter
            var finalFilter = filterBuilder.And(filters);

            var sharePosts = await _sharePosts.Find(finalFilter)
                .SortByDescending(sp => sp.CreatedAt)
                .Limit(pageSize + 1)
                .ToListAsync();

            return sharePosts;
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

            var filterBuilder = Builders<SharePost>.Filter;
            var accIdFilter = filterBuilder.Eq(x => x.AccId, accId);
            var isDeletedFilter = filterBuilder.Eq(sp => sp.IsDeleted, false);
            var scopeStatus = filterBuilder.Eq(sp => sp.Status, 0);

            var finalFilter = filterBuilder.And(accIdFilter, isDeletedFilter, scopeStatus);

            var posts = await _sharePosts.Find(finalFilter).ToListAsync();
            return posts;
        }


        public async Task<List<SharePost>?> GetByPost(string? postId)
        {
            if (string.IsNullOrEmpty(postId))
                return null;

            var filter = Builders<SharePost>.Filter.Eq(x => x.PostId, postId);

            var posts = await _sharePosts.Find(filter).ToListAsync();
            return posts;
        }

        public async Task<SharePost?> CreateAsync(SharePost? request)
        {
            if (request == null)
                return null;

            request.SharePostId = ObjectId.GenerateNewId().ToString();
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

        public async Task<bool> RestoreAsync(string? sharePostId)
        {
            if (string.IsNullOrEmpty(sharePostId)) return false;

            var filter = Builders<SharePost>.Filter.Eq(p => p.SharePostId, sharePostId);
            var update = Builders<SharePost>.Update.Set(p => p.IsDeleted, false)
                                                .Set(p => p.DeletedAt, DateTime.UtcNow);

            var result = await _sharePosts.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DisableAsync(string? postId)
        {
            if (string.IsNullOrEmpty(postId)) return false;

            var filter = Builders<SharePost>.Filter.Eq(p => p.PostId, postId);
            var update = Builders<SharePost>.Update.Set(p => p.Status, 1)
                                                .Set(p => p.DeletedAt, DateTime.UtcNow);

            var result = await _sharePosts.UpdateManyAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<List<SharePost>?> GetDeletedByAccId(string? accId)
        {
            if (string.IsNullOrEmpty(accId))
                return null;

            var filterBuilder = Builders<SharePost>.Filter;
            var accIdFilter = filterBuilder.Eq(x => x.AccId, accId);
            var isDeletedFilter = filterBuilder.Eq(sp => sp.IsDeleted, true);
            var scopeStatus = filterBuilder.Eq(sp => sp.Status, 0);

            // Kết hợp tất cả filter
            var finalFilter = filterBuilder.And(accIdFilter, isDeletedFilter, scopeStatus);

            var posts = await _sharePosts.Find(finalFilter).ToListAsync();
            return posts;
        }
    }
}
