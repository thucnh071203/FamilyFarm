using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class SavedPostService : ISavedPostService
    {
        private readonly ISavedPostRepository _savedPostRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostImageRepository _postImageRepository;
        private readonly IPostCategoryRepository _postCategoryRepository;
        private readonly IHashTagRepository _hashTagRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly IMapper _mapper;

        public SavedPostService(ISavedPostRepository savedPostRepository, IAccountRepository accountRepository, IPostRepository postRepository, IMapper mapper, IPostTagRepository postTagRepository, IHashTagRepository hashTagRepository, IPostCategoryRepository postCategoryRepository, IPostImageRepository postImageRepository)
        {
            _savedPostRepository = savedPostRepository;
            _accountRepository = accountRepository;
            _postRepository = postRepository;
            _mapper = mapper;
            _postTagRepository = postTagRepository;
            _hashTagRepository = hashTagRepository;
            _postCategoryRepository = postCategoryRepository;
            _postImageRepository = postImageRepository;
        }

        public async Task<bool?> CheckIsSavedPost(string? accId, string? postId)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(accId))
            {
                return null;
            }

            return await _savedPostRepository.CheckSavedPost(accId, postId);
        }

        public async Task<ListPostResponseDTO?> ListSavedPostOfAccount(string? accId)
        {
            if (string.IsNullOrEmpty(accId))
                return null;

            var listSavedPost = await _savedPostRepository.ListSavedPostOfAccount(accId);
            if (listSavedPost == null || listSavedPost.Count() <= 0)
                return new ListPostResponseDTO
                {
                    Message = "There is no saved post of this account.",
                    Success = false
                };

            var listPostAvailable = new List<Post>(); //Từ danh sách saved post lấy ra danh sách post còn khả dụng
            foreach (var savedPostItem in listSavedPost)
            {
                var postAvailable = await _postRepository.GetPostById(savedPostItem.PostId);

                if(postAvailable == null)
                    continue;

                if(postAvailable.IsDeleted == true)
                    continue;

                listPostAvailable.Add(postAvailable);
            }

            //Duyệt qua ds post khả dụng lấy các thành phần liên quan: Post tag, image, category, hashtag
            if (listPostAvailable.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "There is no saved post of this account.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post trong listPostAvailable
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostAvailable)
            {
                var postMapper = new PostMapper();
                postMapper.Post = post;

                //Lấy thông tin chủ bài viết
                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);

                postMapper.OwnerPost = ownerPost;

                //2.1 Lấy list images cho từng post
                var listImage = await _postImageRepository.GetPostImageByPost(post.PostId);
                if (listImage != null)
                {
                    postMapper.PostImages = listImage;
                }

                //2.2 Lấy list hashtag
                var listHashtag = await _hashTagRepository.GetHashTagByPost(post.PostId);
                if (listHashtag != null)
                {
                    postMapper.HashTags = listHashtag;
                }

                //2.3 Lấy list category
                var listPostCategory = await _postCategoryRepository.GetCategoryByPost(post.PostId);
                if (listPostCategory != null)
                {
                    postMapper.PostCategories = listPostCategory;
                }

                //2.4 Lấy list tag friend
                var listTagFriend = await _postTagRepository.GetPostTagByPost(post.PostId);
                if (listTagFriend != null)
                {
                    postMapper.PostTags = listTagFriend;
                }

                //Add post mappaer vào List post mapper
                data.Add(postMapper);
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post valid is success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }

        public async Task<CreatedSavedPostResponseDTO?> SavedPost(string? accId, string? postId)
        {
            if(string.IsNullOrEmpty(accId))
                return null;

            if(postId == null || postId == null)
                return null;

            var account = await _accountRepository.GetAccountById(accId);
            if (account == null)
                return new CreatedSavedPostResponseDTO
                {
                    Message = "Not found this account.",
                    Success = false
                };

            var savedPostRequest = new SavedPost
            {
                SavedPostId = "", //Để rỗng do trong DAO có tự tạo lại ID
                AccId = accId,
                PostId = postId
            };

            var savedPost = await _savedPostRepository.CreateSavedPost(savedPostRequest);

            if(savedPost == null) 
                return new CreatedSavedPostResponseDTO
                {
                    Message = "An error occurred during execution.",
                    Success = false
                };

            return new CreatedSavedPostResponseDTO
            {
                Message = "Saved post successfully.",
                Success = true,
                Data = new Models.Mapper.SavedPostMapper
                {
                    Post = await _postRepository.GetPostById(savedPost.PostId),
                    Account = _mapper.Map<MyProfileDTO>(await _accountRepository.GetAccountById(savedPost.AccId)),
                    SavedAt = savedPost.SavedAt
                }
            };
        }

        public async Task<bool?> UnsavedPost(string? accId, string? postId)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(accId))
            {
                return null;
            }
            
            return await _savedPostRepository.DeleteSavedPost(accId, postId);
        }
    }
}
