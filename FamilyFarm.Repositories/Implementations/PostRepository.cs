using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDAO _postDAO;
        private readonly ReactionDAO _reactionDAO;
        private readonly CommentDAO _commentDAO;
        public PostRepository(PostDAO postDAO, ReactionDAO reactionDAO, CommentDAO commentDAO)
        {
            _postDAO = postDAO;
            _reactionDAO = reactionDAO;
            _commentDAO = commentDAO;
        }

        public async Task<List<Post>> SearchPostsByKeyword(string keyword)
        {
            return await _postDAO.SearchPostsByKeywordAsync(keyword);
        }

        public async Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic)
        {
            return await _postDAO.SearchPostsByCategoriesAsync(categoryIds, isAndLogic);
        }

        public async Task<Post?> CreatePost(Post? post)
        {
            return await _postDAO.CreatePost(post);
        }

        public async Task<Post?> UpdatePost(Post? post)
        {
            return await _postDAO.UpdatePost(post);
        }

        public async Task<Post?> GetPostById(string? post_id)
        {
            return await _postDAO.GetById(post_id);
        }

        public async Task<bool> DeletePost(string? post_id)
        {
            return await _postDAO.DeletePost(post_id);
        }

        public async Task<bool> InactivePost(string? post_id)
        {
            return await _postDAO.InactivePost(post_id);
        }

        public async Task<bool> ActivePost(string? post_id)
        {
            return await _postDAO.ActivePost(post_id);
        }

        public async Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword)
        {
            return await _postDAO.SearchPostsInGroupAsync(groupId, keyword);
        }
        public async Task<SearchPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword)
        {
            return await _postDAO.SearchPostsWithAccountAsync(groupId, keyword);
        }

        public async Task<List<Post>?> GetListPost(int is_deleted)
        {
            return await _postDAO.GetListPost(is_deleted, null);
        }

        public async Task<(List<Post> posts, bool hasMore)> GetPaginatedPosts(string? last_post_id, int page_size)
        {
            var posts = await _postDAO.GetListInfinitePost(last_post_id, page_size);
            var hasMore = posts.Count > page_size;
            var paginatedPosts = hasMore ? posts.Take(page_size).ToList() : posts;
            return (paginatedPosts, hasMore);
        }
        public async Task<(List<Post>, bool)> GetListPostInYourGroup(string? lastPostId, int pageSize, List<string> groupIds)
        {
            var posts = await _postDAO.GetListPostInYourGroup(lastPostId, pageSize, groupIds);

            var hasMore = posts.Count > pageSize;
            var paginatedPosts = hasMore ? posts.Take(pageSize).ToList() : posts;

            return (paginatedPosts, hasMore);
        }
        public async Task<(List<Post>, bool)> GetListPostInYourGroupDetail(string? lastPostId, int pageSize, string groupId)
        {
            var posts = await _postDAO.GetListPostInGroupDetail(lastPostId, pageSize, groupId);

            var hasMore = posts.Count > pageSize;
            var paginatedPosts = hasMore ? posts.Take(pageSize).ToList() : posts;

            return (paginatedPosts, hasMore);
        }

        public async Task<List<Post>?> GetListPostCheckedByAI()
        {
            return await _postDAO.GetListPostCheckedByAI();
        }

        public async Task<List<Post>?> GetByAccId(string? accId)
        {
            return await _postDAO.GetByAccId(accId);
        }

        public async Task<List<Post>?> GetDeletedByAccId(string? accId)
        {
            return await _postDAO.GetDeletedByAccId(accId);
        }

        public async Task<List<Post>?> GetListPostByAccId(string? accId, string? privacy)
        {
            return await _postDAO.GetPostsByAccId(accId, privacy);
        }
    }
}
