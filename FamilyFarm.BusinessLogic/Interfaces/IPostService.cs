using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
        Task<ListPostResponseDTO?> SearchPosts(string? keyword, List<string>? categoryIds, bool isAndLogic);
        Task<PostResponseDTO?> AddPost(string? username, CreatePostRequestDTO? request);
        Task<PostResponseDTO?> UpdatePost(string? username, UpdatePostRequestDTO? request);
        Task<PostResponseDTO?> GetPostById(string? postId);
        Task<ListPostResponseDTO> GetPostsByAccId(string? accId);
        Task<DeletePostResponseDTO?> DeletePost(DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> TempDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> RestorePostDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword);
        Task<ListPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword);
        Task<ListPostResponseDTO?> GetListPostValid(); //Lấy các bài post còn khả dụng
        Task<ListPostResponseDTO?> GetListPostDeleted(); //Lấy posts bị xóa
        Task<ListPostResponseDTO?> GetListDeletedPostAndShareByAccount(string? accId);
        Task<ListPostResponseDTO?> GetListAllPost(); //Lấy toàn bộ các bài post
        Task<ListPostResponseDTO?> GetListInfinitePost(string? last_post_id, int page_size);
        Task<ListPostResponseDTO?> GetListInfinitePostAndSharePost(string? lastPostId, string? lastSharePostId, int pageSize);
        Task<ListPostResponseDTO?> GetListPostCheckedAI();
        Task<ListPostResponseDTO?> GetAllPostsForAdmin();
        Task<bool?> CheckPostByAI(string postId);
        Task<ListPostResponseDTO?> GetListDeletedPostByAccount(string? accId);
        Task<ListPostResponseDTO?> GetPostsOwner(string? accId);
        Task<ListPostResponseDTO?> GetPostsOwnerWithShare(string? accId);
        Task<ListPostResponseDTO?> GetPostsPublicByAccId(string? accId);
        Task<ListPostResponseDTO?> GetPostsPublicWithShareByAccId(string? accId);
        Task<ListPostInGroupResponseDTO?> GetPostsInYourGroups(string? last_post_id, int page_size,string accId); //Lấy các bài post còn khả dụng trong các group mà user joined
            //get list post in group detail
        Task<ListPostInGroupResponseDTO?> GetPostsInGroupDetail(string? last_post_id, int page_size, string groupId);
        Task<long> CountPublicPostsInGroupAsync(string groupId);
        Task<List<string>> GetAllImage(string accId);
    }
}
