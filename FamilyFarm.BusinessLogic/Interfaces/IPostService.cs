using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IPostService
    {
        //Task<List<Post>> SearchPostsByKeyword(string keyword);
        //Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic);
        Task<List<Post>> SearchPosts(string? keyword, List<string>? categoryIds, bool isAndLogic);
        Task<CreatePostResponseDTO?> AddPost(string? username, CreatePostRequestDTO? request);
        Task<UpdatePostResponseDTO?> UpdatePost(string? username, UpdatePostRequestDTO? request);
        Task<DeletePostResponseDTO?> DeletePost(string? acc_id, DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> TempDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> RestorePostDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword);
        Task<SearchPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword);
    }
}
