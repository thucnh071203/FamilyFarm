using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace FamilyFarm.DataAccess.DAOs
{
    public class PostDAO
    {
        private readonly IMongoCollection<Post> _post;
        private readonly IMongoCollection<PostCategory> _postCategoryCollection;
        private readonly IMongoCollection<Account> _Account;
        public PostDAO(IMongoDatabase database)
        {
            _post = database.GetCollection<Post>("Post");
            _postCategoryCollection = database.GetCollection<PostCategory>("PostCategory");
            _Account = database.GetCollection<Account>("Account");
        }

        /// <summary>
        /// Searches for posts that contain the given keyword in their content.
        /// This method performs a case-insensitive search for posts that are not deleted 
        /// and contain the specified keyword in their content.
        /// </summary>
        /// <param name="keyword">The keyword to search for in the post content.</param>
        /// <returns>A list of posts that match the search criteria.</returns>
        public async Task<List<Post>> SearchPostsByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // If the keyword is null or whitespace, return an empty list.
                return new List<Post>();
            }

            var filterBuilder = Builders<Post>.Filter;
            // Build a filter to find posts that are not deleted and contain the keyword in the content.
            var filter = filterBuilder.Eq(p => p.IsDeleted, false) &
                         filterBuilder.Regex(p => p.PostContent, new BsonRegularExpression(keyword, "i"));

            // Execute the query on the Post collection and return the results as a list.
            return await _post
                .Find(filter)
                .ToListAsync();
        }

        /// <summary>
        /// Searches for posts that belong to specific categories based on the provided category IDs.
        /// The method allows searching using either an AND logic (posts must belong to all specified categories) 
        /// or an OR logic (posts must belong to at least one specified category).
        /// </summary>
        /// <param name="categoryIds">A list of category IDs to search for.</param>
        /// <param name="isAndLogic">A boolean value indicating whether to use AND logic (true) or OR logic (false) 
        /// for filtering posts based on category membership.</param>
        /// <returns>A list of posts that belong to the specified categories based on the logic used.</returns>
        public async Task<List<Post>> SearchPostsByCategoriesAsync(List<string> categoryIds, bool isAndLogic)
        {
            if (categoryIds == null || categoryIds.Count == 0)
                // If no category IDs are provided, return an empty list.
                return new List<Post>();

            // Validate the categoryIds list to ensure all IDs are valid ObjectId strings.
            foreach (var categoryId in categoryIds)
            {
                if (!ObjectId.TryParse(categoryId, out _))
                    return new List<Post>();
            }

            // Retrieve PostIds from PostCategory collection based on the provided category IDs.
            var postIdGroups = await _postCategoryCollection
                .Find(pc => categoryIds.Contains(pc.CategoryId))
                .Project(pc => new { pc.PostId, pc.CategoryId })
                .ToListAsync();

            if (postIdGroups.Count == 0)
                // If no posts match the provided categories, return an empty list.
                return new List<Post>();

            // Determine valid PostIds based on the AND or OR logic.
            var validPostIds = new List<string>();
            if (isAndLogic)
            {
                // AND logic: Only include posts that belong to all specified categories.
                var postIdCounts = postIdGroups
                    .GroupBy(pc => pc.PostId)
                    .Select(g => new { PostId = g.Key, CategoryCount = g.Count() })
                    .Where(g => g.CategoryCount == categoryIds.Count)
                    .Select(g => g.PostId)
                    .ToList();

                validPostIds = postIdCounts;
            }
            else
            {
                // OR logic: Include posts that belong to at least one of the specified categories.
                validPostIds = postIdGroups.Select(pc => pc.PostId).Distinct().ToList();
            }

            if (validPostIds.Count == 0)
                // If no posts match the category criteria, return an empty list.
                return new List<Post>();

            // Retrieve the posts from the Post collection that match the valid PostIds, are not deleted, and are public.
            return await _post
                .Find(p => validPostIds.Contains(p.PostId) && p.IsDeleted == false && p.PostScope == "Public")
                .ToListAsync();
        }

        /// <summary>
        ///     Create new post (chưa check các validate)
        /// </summary>
        public async Task<Post?> CreatePost(Post? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            request.CreatedAt = DateTime.UtcNow;

            await _post.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Update post
        /// </summary>
        /// <returns>return a new post after updating, if update is fail return null</returns>
        public async Task<Post?> UpdatePost(Post? request)
        {
            if (request == null || string.IsNullOrEmpty(request.PostId))
            {
                return null;
            }

            try
            {
                var filter = Builders<Post>.Filter.Eq(x => x.PostId, request.PostId);

                var update = Builders<Post>.Update
                    .Set(x => x.PostContent, request.PostContent)
                    .Set(x => x.PostScope, request.PostScope)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);
                // Thêm các field khác bạn muốn update ở đây

                var result = await _post.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    // Sau khi update, lấy lại Post mới nhất để trả về
                    var updatedPost = await _post.Find(filter).FirstOrDefaultAsync();
                    return updatedPost;
                }
                else
                {
                    return null; // Không update được (ví dụ postId không tồn tại)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Get a post by post Id
        /// </summary>
        /// <returns>return a post</returns>
        public async Task<Post?> GetById(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return null;

            var filter = Builders<Post>.Filter.Eq(x => x.PostId, post_id);

            try
            {
                var post = await _post.Find(filter).FirstOrDefaultAsync();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///     Delete post with post id
        /// </summary>
        /// <returns>return a status, true if delete SUCCESS, false if delete FAIL</returns>
        public async Task<bool> DeletePost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id)) return false;

            var filter = Builders<Post>.Filter.Eq(p => p.PostId, post_id);
            var result = await _post.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        /// <summary>
        ///     Update post field isDelete = true
        /// </summary>
        /// <returns>return status if a post update value of isDelete = true SUCCESSFULLY</returns>
        public async Task<bool> InactivePost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id)) return false;

            var filter = Builders<Post>.Filter.Eq(p => p.PostId, post_id);
            var update = Builders<Post>.Update.Set(p => p.IsDeleted, true)
                                                .Set(p => p.DeletedAt, DateTime.UtcNow);

            var result = await _post.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        /// <summary>
        ///     Update post field isDelete = false
        /// </summary>
        /// <returns>return status if a post update value of isDelete = false SUCCESSFULLY</returns>
        public async Task<bool> ActivePost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id)) return false;

            var filter = Builders<Post>.Filter.Eq(p => p.PostId, post_id);
            var update = Builders<Post>.Update.Set(p => p.IsDeleted, false);
            var result = await _post.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return new List<Post>();

            var filterBuilder = Builders<Post>.Filter;
            var objectGroupId = ObjectId.Parse(groupId);

            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq("GroupId", objectGroupId),
                Builders<Post>.Filter.Eq("IsDeleted", false),
                Builders<Post>.Filter.Regex("PostContent", new BsonRegularExpression(keyword, "i"))
            );


            return await _post.Find(filter).SortByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<SearchPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return new SearchPostInGroupResponseDTO { Success = false, Message = "GroupId is required." };

            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq("GroupId", groupId),
                Builders<Post>.Filter.Eq("IsDeleted", false),
                Builders<Post>.Filter.Regex("PostContent", new BsonRegularExpression(keyword, "i"))
            );

            var posts = await _post.Find(filter).ToListAsync();

            if (posts.Count == 0)
            {
                return new SearchPostInGroupResponseDTO
                {
                    Success = true,
                    Message = "No posts found.",
                    Posts = new List<PostInGroup>()
                };
            }

            var accIds = posts.Select(p => p.AccId).Distinct().ToList();
            var accFilter = Builders<Account>.Filter.In("_id", accIds.Select(id => ObjectId.Parse(id)));
            var accounts = await _Account.Find(accFilter).ToListAsync();

            var postAndAccList = posts.Select(post =>
            {
                var account = accounts.FirstOrDefault(a => a.AccId.ToString() == post.AccId);
                var minimalAcc = new MiniAccountDTO
                {
                    AccId = account.AccId,
                    FullName = account.FullName,
                    Username = account.Username,
                      Email = account.Email,
                        Avatar = account.Avatar
                };

                return new PostInGroup
                {
                    post = post,
                    account = minimalAcc
                };
            }).ToList();

            return new SearchPostInGroupResponseDTO
            {
                Success = true,
                Message = "Found posts.",
                Posts = postAndAccList
            };
        }

        /// <summary>
        ///     List all post Valid with condition 
        /// </summary>
        /// <param name="isDeleted">
        ///     0 = only not deleted, 
        ///     1 = only deleted, 
        ///     -1 or others = all posts
        /// </param>
        /// <returns>return list Post object has sorted based on Created At</returns>
        public async Task<List<Post>?> GetListPost(int isDeleted)
        {
            FilterDefinition<Post> filter;

            if (isDeleted == 0)
            {
                filter = Builders<Post>.Filter.Eq(p => p.IsDeleted, false);
            }
            else if (isDeleted == 1)
            {
                filter = Builders<Post>.Filter.Eq(p => p.IsDeleted, true);
            }
            else
            {
                filter = Builders<Post>.Filter.Empty;
            }

            var posts = await _post.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts;
        }

        /// <summary>
        ///     List all post valid infinite scroll
        /// </summary>
        /// <param name="lastPostId">Last post of list post before</param>
        /// <param name="pageSize"></param>
        /// <returns>return list Post object has sorted based on Created At and paging</returns>
        public async Task<List<Post>> GetListInfinitePost(string? lastPostId, int pageSize)
        {
            // Điều kiện lọc theo isDeleted = false
            var isDeletedFilter = Builders<Post>.Filter.Eq(p => p.IsDeleted, false);

            FilterDefinition<Post> finalFilter;

            if (!string.IsNullOrEmpty(lastPostId))
            {
                // Lọc theo Id < lastPostId và isDeleted
                var lastIdFilter = Builders<Post>.Filter.Lt(p => p.PostId, lastPostId);
                finalFilter = Builders<Post>.Filter.And(isDeletedFilter, lastIdFilter);
            }
            else
            {
                // Chỉ lọc theo isDeleted
                finalFilter = isDeletedFilter;
            }

            var posts = await _post.Find(finalFilter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(pageSize + 1)
                .ToListAsync();

            return posts;
        }
    }
}
