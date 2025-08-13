using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
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


        public async Task<ListSharePostResponseDTO?> GetSharePostsByAccId(string? accId)
        {
            if (string.IsNullOrEmpty(accId)|| !ObjectId.TryParse(accId, out _)) 
                return null;

            var sharePosts = await _sharePostRepository.GetByAccId(accId);

            if (sharePosts == null)
            {
                return new ListSharePostResponseDTO
                {
                    Success = false,
                    Message = "Get list share posts fail!",
                    Count = 0,
                    SharePostDatas = null
                };
            }

            if (!sharePosts.Any())
            {
                return new ListSharePostResponseDTO
                {
                    Success = false,
                    Message = "No share posts found!",
                    Count = 0,
                    SharePostDatas = null
                };
            }

            var sharePostDatas = new List<SharePostDTO>();
            foreach (var sharePost in sharePosts)
            {
                var post = await _postService.GetPostById(sharePost.PostId);
                sharePostDatas.Add(new SharePostDTO
                {
                    SharePost = await _sharePostRepository.GetById(sharePost.SharePostId),
                    SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                    PostData = post?.Data
                });
            }
            return new ListSharePostResponseDTO
            {
                Success = true,
                Message = "Get list Share post successfully!",
                Count = sharePostDatas.Count(s => s.SharePost?.IsDeleted != true && s.PostData?.Post?.IsDeleted != true),
                SharePostDatas = sharePostDatas.Where(s => s.SharePost?.IsDeleted != true && s.PostData?.Post?.IsDeleted != true).ToList()
            };
        }

        public async Task<ListSharePostResponseDTO?> GetSharePostsByPostId(string? postId)
        {
            if (string.IsNullOrEmpty(postId) || !ObjectId.TryParse(postId, out _))
                return null;

            var sharePosts = await _sharePostRepository.GetByPost(postId);

            if (sharePosts == null)
            {
                return new ListSharePostResponseDTO
                {
                    Success = false,
                    Message = "Get list share posts fail!",
                    Count = 0,
                    SharePostDatas = null
                };
            }

            if (!sharePosts.Any())
            {
                return new ListSharePostResponseDTO
                {
                    Success = false,
                    Message = "No share posts found for this post!",
                    Count = 0,
                    SharePostDatas = null
                };
            }

            var sharePostDatas = new List<SharePostDTO>();
            foreach (var sharePost in sharePosts)
            {
                var post = await _postService.GetPostById(sharePost.PostId);
                sharePostDatas.Add(new SharePostDTO
                {
                    SharePost = await _sharePostRepository.GetById(sharePost.SharePostId),
                    SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                    PostData = post?.Data
                });
            }
            return new ListSharePostResponseDTO
            {
                Success = true,
                Message = "Get list Share post by post ID successfully!",
                Count = sharePostDatas.Count(s => s.SharePost?.IsDeleted != true && s.PostData?.Post?.IsDeleted != true),
                SharePostDatas = sharePostDatas.Where(s => s.SharePost?.IsDeleted != true && s.PostData?.Post?.IsDeleted != true).ToList()
            };
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

            //bool AICheck = await _cohereService.IsAgricultureRelatedAsync(request.SharePostContent);

            var sharePost = new SharePost();
            sharePost.PostId = request.PostId;
            sharePost.SharePostContent = request.SharePostContent;
            sharePost.SharePostScope = request.SharePostScope;
            sharePost.AccId = ownAccount.AccId;
            sharePost.CreatedAt = DateTime.UtcNow;
            sharePost.Status = 0;
            //if (AICheck)
            //{
            //    sharePost.Status = 0;
            //}
            //else
            //{
            //    sharePost.Status = 1;
            //}

            var newSharePost = await _sharePostRepository.CreateAsync(sharePost);

            if (newSharePost == null)
                return new SharePostResponseDTO
                {
                    Message = "Share post fail.",
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

                    var sharePostTag = new SharePostTag();
                    sharePostTag.AccId = account.AccId;
                    sharePostTag.SharePostId = newSharePost.SharePostId;
                    sharePostTag.Fullname = account.FullName;
                    sharePostTag.Username = account.Username;
                    sharePostTag.Avatar = account.Avatar;
                    sharePostTag.CreatedAt = DateTime.UtcNow;

                    var newSharePostTag = await _sharePostTagRepository.CreateAsyns(sharePostTag);
                    if (newSharePostTag != null)
                        sharePostTags.Add(newSharePostTag);
                }
            }

            var post = await _postService.GetPostById(request.PostId);

            if (post?.Success == false)
                return new SharePostResponseDTO
                {
                    Message = "No posts found for share.",
                    Success = false
                };

            SharePostDTO sharePostData = new SharePostDTO();
            sharePostData.SharePost = newSharePost;
            sharePostData.SharePostTags = sharePostTags;
            sharePostData.HashTags = hashTags;
            sharePostData.PostData = post?.Data;

            return new SharePostResponseDTO
            {
                Message = "Share post successfully.",
                Success = true,
                SharePostData = sharePostData
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharePostId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SharePostResponseDTO?> UpdateSharePost(string? sharePostId, UpdateSharePostRequestDTO? request)
        {
            if (sharePostId == null || request == null)
                return null;

            var sharePost = await _sharePostRepository.GetById(sharePostId);
            if (sharePost == null)
                return null;

            // 1. Update nội dung cơ bản
            sharePost.SharePostContent = request.SharePostContent;
            sharePost.SharePostScope = request.SharePostScope;
            sharePost.UpdatedAt = DateTime.UtcNow;

            var newSharePost = await _sharePostRepository.UpdateAsync(sharePost);
            if (newSharePost == null)
            {
                return new SharePostResponseDTO
                {
                    Success = false,
                    Message = "Update share post failed."
                };
            }

            // 2. Hashtag
            if (request.IsDeleteAllHashtag == true)
            {
                await _hashTagRepository.DeleteAllByPostId(newSharePost.SharePostId);
            }

            if (request.HashTagToRemove != null && request.HashTagToRemove.Count > 0)
            {
                foreach (var tagId in request.HashTagToRemove)
                {
                    await _hashTagRepository.DeleteHashTagById(tagId);
                }
            }

            List<HashTag> hashTags = new();
            if (request.HashTagToAdd != null && request.HashTagToAdd.Count > 0)
            {
                foreach (var tag in request.HashTagToAdd)
                {
                    var hashTag = new HashTag
                    {
                        HashTagContent = tag,
                        PostId = newSharePost.SharePostId,
                        CreateAt = DateTime.UtcNow
                    };

                    var newHashTag = await _hashTagRepository.CreateHashTag(hashTag);
                    if (newHashTag != null)
                        hashTags.Add(newHashTag);
                }
            }

            // 3. Post Tag (Tag bạn bè)
            if (request.IsDeleteAllFriend == true)
            {
                await _sharePostTagRepository.DeleteAllBySharePostId(newSharePost.SharePostId);
            }

            if (request.TagFiendIdsToRemove != null && request.TagFiendIdsToRemove.Count > 0)
            {
                foreach (var id in request.TagFiendIdsToRemove)
                {
                    await _sharePostTagRepository.DeleteTagById(id);
                }
            }

            List<SharePostTag> sharePostTags = new();
            if (request.TagFiendIdsToAdd != null && request.TagFiendIdsToAdd.Count > 0)
            {
                foreach (var friendId in request.TagFiendIdsToAdd)
                {
                    var account = await _accountRepository.GetAccountById(friendId);
                    if (account == null)
                        continue;

                    var tag = new SharePostTag
                    {
                        AccId = account.AccId,
                        SharePostId = newSharePost.SharePostId,
                        Fullname = account.FullName,
                        Username = account.Username,
                        Avatar = account.Avatar,
                        CreatedAt = DateTime.UtcNow
                    };

                    var newTag = await _sharePostTagRepository.CreateAsyns(tag);
                    if (newTag != null)
                        sharePostTags.Add(newTag);
                }
            }

            var post = await _postService.GetPostById(newSharePost.PostId);

            if (post?.Success == false)
                return new SharePostResponseDTO
                {
                    Message = "No posts found for share.",
                    Success = false
                };

            // 4. Return response
            SharePostDTO sharePostData = new SharePostDTO();
            sharePostData.SharePost = newSharePost;
            sharePostData.SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePostId);
            sharePostData.HashTags = await _hashTagRepository.GetHashTagByPost(sharePostId);
            sharePostData.PostData = post?.Data;

            return new SharePostResponseDTO
            {
                Message = "Share post successfully.",
                Success = true,
                SharePostData = sharePostData
            };
        }


        public async Task<SharePostResponseDTO?> HardDeleteSharePost(string? sharePostId)
        {
            var sharePost = await _sharePostRepository.GetById(sharePostId);

            if (sharePost == null)
                return new SharePostResponseDTO
                {
                    Message = "Not found this share post!",
                    Success = false
                };

            var isDeleted = await _sharePostRepository.HardDeleteAsync(sharePostId);

            if (isDeleted == false)
            {
                return new SharePostResponseDTO
                {
                    Message = "Delete share post fail!",
                    Success = false
                };
            }

            //Xóa cứng toàn bộ các bảng liên quan
            await _sharePostTagRepository.DeleteAllBySharePostId(sharePostId);
            await _hashTagRepository.DeleteAllByPostId(sharePostId);

            return new SharePostResponseDTO
            {
                Message = "Delete share post successfully!",
                Success = true
            };
        }

        public async Task<SharePostResponseDTO?> SoftDeleteSharePost(string? sharePostId)
        {
            var sharePost = await _sharePostRepository.GetById(sharePostId);

            if (sharePost == null)
                return new SharePostResponseDTO
                {
                    Message = "Not found this share post!",
                    Success = false
                };

            //Kiểm tra xem có xóa mềm chưa, nếu xóa mềm rồi và thời gian hơn 30 ngày thì không xóa nữa
            if (sharePost.DeletedAt.HasValue && (DateTime.UtcNow - sharePost.DeletedAt.Value).TotalDays >= 30)
            {
                return new SharePostResponseDTO
                {
                    Message = "Share post has been moved to bin!",
                    Success = false
                };
            }

            var isSoftDelete = await _sharePostRepository.SoftDeleteAsync(sharePostId);

            if (isSoftDelete == false)
            {
                return new SharePostResponseDTO
                {
                    Message = "Move to bin share post fail!",
                    Success = false
                };
            }
            return new SharePostResponseDTO
            {
                Message = "Move to bin share post successfully!",
                Success = true
            };
        }

        public async Task<SharePostResponseDTO?> RestoreSharePost(string? sharePostId)
        {
            var sharePost = await _sharePostRepository.GetById(sharePostId);

            if (sharePost == null)
                return new SharePostResponseDTO
                {
                    Message = "Not found this share post!",
                    Success = false
                };

            var isRestore = await _sharePostRepository.RestoreAsync(sharePostId);

            if (isRestore == false)
            {
                return new SharePostResponseDTO
                {
                    Message = "Restore share post fail!",
                    Success = false
                };
            }
            return new SharePostResponseDTO
            {
                Message = "Restore share post successfully!",
                Success = true
            };
        }
    }
}
