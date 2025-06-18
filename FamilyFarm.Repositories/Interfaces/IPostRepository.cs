using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> SearchPostsByKeyword(string keyword);
        Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic);
        Task<Post?> CreatePost(Post? post);
        Task<Post?> UpdatePost(Post? post);
        Task<bool> DeletePost(string? post_id);
        Task<bool> InactivePost(string? post_id);
        Task<bool> ActivePost(string? post_id);
        Task<Post?> GetPostById(string? post_id);
        Task<List<Post>?> GetByAccId(string? accId);
        Task<List<Post>?> GetDeletedByAccId(string? accId);
        Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword);
        Task<SearchPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword);
        Task<List<Post>?> GetListPost(int is_deleted);
        Task<(List<Post> posts, bool hasMore)> GetPaginatedPosts(string? last_post_id, int page_size);
        Task<List<Post>?> GetListPostCheckedByAI();
        Task<List<Post>?> GetListPostByAccId(string? accId, string? privacy);
    }
}
