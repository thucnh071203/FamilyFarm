﻿using FamilyFarm.Models.DTOs.Request;
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
        Task<List<Post>> SearchPosts(string? keyword, List<string>? categoryIds, bool isAndLogic);
        Task<PostResponseDTO?> AddPost(string? username, CreatePostRequestDTO? request);
        Task<PostResponseDTO?> UpdatePost(string? username, UpdatePostRequestDTO? request);
        Task<PostResponseDTO?> GetPostById(string? postId);
        Task<ListPostResponseDTO> GetPostsByAccId(string? accId);
        Task<DeletePostResponseDTO?> DeletePost(string? acc_id, DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> TempDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<DeletePostResponseDTO?> RestorePostDeleted(string? acc_id, DeletePostRequestDTO request);
        Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword);
        Task<SearchPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword);
        Task<ListPostResponseDTO?> GetListPostValid(); //Lấy các bài post còn khả dụng
        Task<ListPostResponseDTO?> GetListPostDeleted(); //Lấy posts bị xóa
        Task<ListPostResponseDTO?> GetListAllPost(); //Lấy toàn bộ các bài post
        Task<ListPostResponseDTO?> GetListInfinitePost(string? last_post_id, int page_size);
        Task<ListPostResponseDTO?> GetListPostCheckedAI();
        Task<bool?> CheckPostByAI(string postId);
        Task<ListPostResponseDTO?> GetListDeletedPostByAccount(string? accId);

    }
}
