using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class SharePostService : ISharePostService
    {
        private readonly ISharePostRepository _sharePostRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IHashTagRepository _hashTagRepository;
        private readonly ISharePostTagRepository _sharePostTagRepository;
        private readonly ICohereService _cohereService;
        private readonly IPostService _postService;

        public SharePostService(ISharePostRepository sharePostRepository, IAccountRepository accountRepository, IHashTagRepository hashTagRepository, ISharePostTagRepository sharePostTagRepository, ICohereService cohereService, IPostService postService)
        {
            _sharePostRepository = sharePostRepository;
            _accountRepository = accountRepository;
            _hashTagRepository = hashTagRepository;
            _sharePostTagRepository = sharePostTagRepository;
            _cohereService = cohereService;
            _postService = postService;
        }

        /// <summary>
        /// Creates a new share post by a given account, optionally tagging friends and adding hashtags.
        /// Also uses an AI service to check if the post content is agriculture-related to determine its status.
        /// </summary>
        /// <param name="accId">The ID of the account creating the share post.</param>
        /// <param name="request">The request DTO containing content, scope, hashtags, and tagged friend IDs.</param>
        /// <returns>
        /// Returns a <see cref="SharePostResponseDTO"/> indicating whether the post was created successfully.
        /// Includes the created share post data along with its associated hashtags and tagged friends.
        /// Returns null if the account or request is invalid.
        /// </returns>
        public async Task<SharePostResponseDTO?> CreateSharePost(string? accId, SharePostRequestDTO? request)
        {
            if (request == null)
                return null;

            if (accId == null)
                return null;

            var ownAccount = await _accountRepository.GetAccountByIdAsync(accId);
            if (ownAccount == null)
                return null;

            bool AICheck = await _cohereService.IsAgricultureRelatedAsync(request.SharePostContent);

            var sharePostRequest = new SharePost();
            sharePostRequest.SharePostContent = request.SharePostContent;
            sharePostRequest.SharePostScope = request.SharePostScope;
            sharePostRequest.AccId = ownAccount.AccId;
            sharePostRequest.CreatedAt = DateTime.UtcNow;

            if (AICheck)
            {
                sharePostRequest.Status = 0;
            }
            else
            {
                sharePostRequest.Status = 1;
            }

            var newSharePost = await _sharePostRepository.CreateAsync(sharePostRequest);

            if (newSharePost == null)
                return new SharePostResponseDTO
                {
                    Message = "Create post is fail.",
                    Success = false
                };

            List<HashTag> hashTags = new List<HashTag>();

            if (request.HashTags != null && request.HashTags.Count > 0)
            {
                foreach (var itemHashtag in request.HashTags)
                {
                    var hashtag = new HashTag();
                    hashtag.HashTagContent = itemHashtag;
                    hashtag.PostId = newSharePost.SharePostId;
                    hashtag.CreateAt = DateTime.UtcNow;

                    var newHashTag = await _hashTagRepository.CreateHashTag(hashtag);

                    if (newHashTag != null)
                        hashTags.Add(newHashTag);
                }
            }

            List<SharePostTag> sharePostTags = new List<SharePostTag>();

            if (request.TagFiendIds != null && request.TagFiendIds.Count > 0)
            {
                foreach (var friendId in request.TagFiendIds)
                {
                    var account = await _accountRepository.GetAccountById(friendId);
                    if (account == null)
                        continue;

                    var postTag = new SharePostTag();
                    postTag.AccId = account.AccId;
                    postTag.SharePostId = newSharePost.SharePostId;
                    postTag.CreatedAt = DateTime.UtcNow;

                    var newSharePostTag = await _sharePostTagRepository.CreateAsyns(postTag);
                    if (newSharePostTag != null)
                        sharePostTags.Add(newSharePostTag);
                }
            }


            var post = await _postService.GetPostById(request.PostId);

            SharePostDTO sharePostData = new SharePostDTO();
            sharePostData.SharePost = newSharePost;
            sharePostData.SharePostTags = sharePostTags;
            sharePostData.HashTags = hashTags;
            sharePostData.PostData = post?.Data;

            return new SharePostResponseDTO
            {
                Message = "Create post is successfully.",
                Success = true,
                Data = sharePostData
            };
        }
    }
}
