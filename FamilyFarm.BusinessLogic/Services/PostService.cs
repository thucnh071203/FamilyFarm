using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FamilyFarm.BusinessLogic.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostCategoryRepository _postCategoryRepository;
        private readonly IPostImageRepository _postImageRepository;
        private readonly IHashTagRepository _hashTagRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly ICategoryPostRepository _categoryPostRepository;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAccountRepository _accountRepository;
        private readonly ICohereService _cohereService;
        private readonly IMapper _mapper;
        private readonly IReactionRepository _reactionRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ISharePostRepository _sharePostRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ISharePostTagRepository _sharePostTagRepository;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        private readonly IStatisticRepository _statisticRepository;
        private readonly INotificationService _notificationService;

        public PostService(IPostRepository postRepository, IPostCategoryRepository postCategoryRepository, IPostImageRepository postImageRepository, IHashTagRepository hashTagRepository, IPostTagRepository postTagRepository, ICategoryPostRepository categoryPostRepository, IUploadFileService uploadFileService, IAccountRepository accountRepository, ICohereService cohereService, IMapper mapper, IReactionRepository reactionRepository, ICommentRepository commentRepository, ISharePostRepository sharePostRepository, IGroupRepository groupRepository, ISharePostTagRepository sharePostTagRepository, IHubContext<TopEngagedPostHub> hubContext, IStatisticRepository statisticRepository, INotificationRepository notificationRepository, INotificationService notificationService)
        {
            _postRepository = postRepository;
            _postCategoryRepository = postCategoryRepository;
            _postImageRepository = postImageRepository;
            _hashTagRepository = hashTagRepository;
            _postTagRepository = postTagRepository;
            _categoryPostRepository = categoryPostRepository;
            _uploadFileService = uploadFileService;
            _accountRepository = accountRepository;
            _cohereService = cohereService;
            _mapper = mapper;
            _reactionRepository = reactionRepository;
            _commentRepository = commentRepository;
            _sharePostRepository = sharePostRepository;
            _groupRepository = groupRepository;
            _sharePostTagRepository = sharePostTagRepository;
            _hubContext = hubContext;
            _statisticRepository = statisticRepository;
            _notificationService = notificationService;
        }

        /// <summary>
        ///     Add new post
        /// </summary>
        public async Task<PostResponseDTO?> AddPost(string? username, CreatePostRequestDTO? request)
        {

            //Kiem tra dau vao, PostId tu dong nen khong can kiem tra
            if (request == null)
                return null;

            //1. Add post voi thong tin co ban:
            if (username == null)
                return null;

            var ownAccount = await _accountRepository.GetAccountByUsername(username);
            if (ownAccount == null)
                return null;
            bool AICheck = await _cohereService.IsAgricultureRelatedAsync(request.PostContent);

           // bool AICheck = true;

            var postRequest = new Post();
            postRequest.PostContent = request.PostContent;
            postRequest.PostScope = request.Privacy;
            postRequest.AccId = ownAccount.AccId;
            postRequest.CreatedAt = DateTime.UtcNow;
            postRequest.IsInGroup = request.isInGroup;
            postRequest.GroupId = request.GroupId;
            
           
            if (AICheck)//if true, status is 0, mean that content is Agriculture
            {
                postRequest.Status = 0;
            }
            else
            {
                postRequest.Status = 1;//if false, status is 1, mean that content is not Agriculture
            }

            var newPost = await _postRepository.CreatePost(postRequest);

            long totalPost = await _statisticRepository.CountPostsAsync();
            await _hubContext.Clients.All.SendAsync("NewPost", totalPost);


            if (newPost == null)
                return new PostResponseDTO
                {
                    Message = "Create post is fail.",
                    Success = false
                };

            //2. Add Post Category
            List<PostCategory> postCategories = new List<PostCategory>();

            if (request.ListCategoryOfPost != null && request.ListCategoryOfPost.Count > 0)
            {
                foreach (var categoryId in request.ListCategoryOfPost)
                {
                    //Lay thong tin chi tiet cua tung Category
                    var category = await _categoryPostRepository.GetCategoryById(categoryId);

                    if (category != null)
                    {
                        //Goi ham add tung category vao bang PostCategory
                        var itemCategory = new PostCategory();
                        itemCategory.CategoryId = categoryId;
                        itemCategory.CategoryName = category.CategoryName;
                        itemCategory.PostId = newPost.PostId;
                        itemCategory.CreatedAt = DateTime.UtcNow;

                        var postCategory = await _postCategoryRepository.CreatePostCategory(itemCategory);

                        if (postCategory != null)
                            postCategories.Add(postCategory);
                    }
                }
            }

            //3. Add post images
            List<PostImage> postImages = new List<PostImage>();

            if (request.ListImage != null && request.ListImage.Count > 0)
            {
                //Goi method upload List image tu Upload file service
                List<FileUploadResponseDTO> listImageUrl = await _uploadFileService.UploadListImage(request.ListImage);

                if (listImageUrl != null && listImageUrl.Count > 0)
                {
                    foreach (var image in listImageUrl)
                    {
                        var postImage = new PostImage();
                        postImage.PostId = newPost.PostId;
                        postImage.ImageUrl = image.UrlFile ?? "";

                        var newPostImage = await _postImageRepository.CreatePostImage(postImage);

                        if (newPostImage != null)
                            postImages.Add(newPostImage);
                    }
                }
            }

            //4. Add HashTag
            List<HashTag> hashTags = new List<HashTag>();

            if (request.Hashtags != null && request.Hashtags.Count > 0)
            {
                foreach (var itemHashtag in request.Hashtags)
                {
                    var hashtag = new HashTag();
                    hashtag.HashTagContent = itemHashtag;
                    hashtag.PostId = newPost.PostId;
                    hashtag.CreateAt = DateTime.UtcNow;

                    var newHashTag = await _hashTagRepository.CreateHashTag(hashtag);

                    if (newHashTag != null)
                        hashTags.Add(newHashTag);
                }
            }

            //5. Add Post tag
            List<PostTag> postTags = new List<PostTag>();

            if (request.ListTagFriend != null && request.ListTagFriend.Count > 0)
            {
                foreach (var friendId in request.ListTagFriend)
                {
                    var account = await _accountRepository.GetAccountById(friendId);
                    if (account == null)
                        continue;

                    var postTag = new PostTag();
                    postTag.AccId = account.AccId;
                    postTag.Fullname = account.FullName;
                    postTag.Username = account.Username;
                    postTag.PostId = newPost.PostId;
                    postTag.CreatedAt = DateTime.UtcNow;

                    var newPostTag = await _postTagRepository.CreatePostTag(postTag);
                    if (newPostTag != null)
                        postTags.Add(newPostTag);

                    
                }

                var notiRequest = new SendNotificationRequestDTO
                {
                    ReceiverIds = request.ListTagFriend,
                    SenderId = ownAccount.AccId,
                    CategoryNotiId = "685d3f6d1d2b7e9f45ae1c38",
                    TargetId = newPost.PostId,
                    TargetType = "Post",
                    Content = ownAccount.FullName + " tagged you in a post."
                };

                var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);
            }

            //Tạo data trả về
            PostMapper data = new PostMapper();
            data.Post = newPost;
            data.PostTags = postTags;
            data.OwnerPost = _mapper.Map<MyProfileDTO>(ownAccount);
            data.PostCategories = postCategories;
            data.PostImages = postImages;
            data.HashTags = hashTags;

            if (request.isInGroup == true)
            {
                data.Group = await _groupRepository.GetGroupById(postRequest.GroupId);
            }

            return new PostResponseDTO
            {
                Message = "Create post is successfully.",
                Success = true,
                Data = data
            };
        }

        

        /// <summary>
        /// Retrieves a post and its related data by the post's unique identifier.
        /// </summary>
        /// <param name="postId">The unique ID of the post to retrieve.</param>
        /// <returns>
        /// Returns a <see cref="PostResponseDTO"/> containing the post details,
        /// including categories, images, hashtags, and tags. Returns a failure response
        /// if the post is not found.
        /// </returns>
        public async Task<PostResponseDTO?> GetPostById(string? postId)
        {
            var post = await _postRepository.GetPostById(postId);

            if (post == null)
            {
                return new PostResponseDTO
                {
                    Success = false,
                    Message = "No post!",
                };
            }

            var reactions = await _reactionRepository.GetAllByEntityAsync(postId, "Post");
            var comments = await _commentRepository.GetAllByPost(postId);
            var shares = await _sharePostRepository.GetByPost(postId);
            var account = await _accountRepository.GetAccountById(post.AccId);
            var ownerPost = _mapper.Map<MyProfileDTO>(account);

            PostMapper postData = new PostMapper
            {
                ReactionCount = reactions.Count,
                CommentCount = comments.Count,
                ShareCount = shares?.Count,
                Post = post,
                PostCategories = await _postCategoryRepository.GetCategoryByPost(postId),
                PostImages = await _postImageRepository.GetPostImageByPost(postId),
                HashTags = await _hashTagRepository.GetHashTagByPost(postId),
                PostTags = await _postTagRepository.GetPostTagByPost(postId),
                OwnerPost = ownerPost,
                
            };

            return new PostResponseDTO
            {
                Success = true,
                Message = "Get post successfully!",
                Data = postData
            };
        }


        /// <summary>
        /// Retrieves all posts created by a specific account, including related data for each post.
        /// </summary>
        /// <param name="accId">The account ID used to filter posts.</param>
        /// <returns>
        /// Returns a <see cref="ListPostResponseDTO"/> containing a list of posts with their
        /// associated categories, images, hashtags, and tags. Returns a message if no posts exist or if the retrieval fails.
        /// </returns>
        public async Task<ListPostResponseDTO> GetPostsByAccId(string? accId)
        {
            // List post by account id
            var posts = await _postRepository.GetByAccId(accId);

            // check if post is null
            if (posts == null)
            {
                return new ListPostResponseDTO
                {
                    Success = false,
                    Message = "Get list posts fail!",
                    Count = 0,
                    Data = null
                };
            }

            if (!posts.Any())
            {
                return new ListPostResponseDTO
                {
                    Success = true,
                    Message = "No posts yet!",
                    Count = 0,
                    Data = null
                };
            }

            var postDatas = new List<PostMapper>();

            foreach (var post in posts)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);
                postDatas.Add(new PostMapper
                {
                    ReactionCount = reactions.Count,
                    CommentCount = comments.Count,
                    ShareCount = shares?.Count,
                    Post = await _postRepository.GetPostById(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId)
                });
            }

            return new ListPostResponseDTO
            {
                Success = true,
                Message = "Get list posts successfully!",
                Count = postDatas.Count,
                Data = postDatas
            };
        }


        /// <summary>
        ///     Hard Delete a post - delete out of DB
        /// </summary>
        public async Task<DeletePostResponseDTO?> DeletePost(DeletePostRequestDTO request)
        {
            //if (acc_id == null)
            //    return null;

            if (request.PostId == null)
                return null;

            var post = await _postRepository.GetPostById(request.PostId);

            if (post == null)
                return new DeletePostResponseDTO
                {
                    Message = "Not found this post",
                    Success = false
                };

            //if (post.AccId != acc_id)
            //    return new DeletePostResponseDTO
            //    {
            //        Message = "You are not permission for this action.",
            //        Success = false
            //    };

            var isDeleted = await _postRepository.DeletePost(post.PostId);

            if (isDeleted == false)
            {
                return new DeletePostResponseDTO
                {
                    Message = "Delete post fail.",
                    Success = false
                };
            }

            //Xóa cứng toàn bộ các bảng liên quan
            await _postImageRepository.DeleteAllByPostId(post.PostId);
            await _postCategoryRepository.DeleteAllByPostId(post.PostId);
            await _postTagRepository.DeleteAllByPostId(post.PostId);
            await _hashTagRepository.DeleteAllByPostId(post.PostId);

            return new DeletePostResponseDTO
            {
                Message = "Delete post successfully.",
                Success = true
            };
        }

        /// <summary>
        ///     Restore a post from recycl bin - when this post is not yet hard deleted.
        /// </summary>
        public async Task<DeletePostResponseDTO?> RestorePostDeleted(string? acc_id, DeletePostRequestDTO request)
        {
            if (acc_id == null)
                return null;

            if (request == null)
                return null;

            if (request.PostId == null)
                return null;

            var post = await _postRepository.GetPostById(request.PostId);

            if (post == null)
            {
                return new DeletePostResponseDTO
                {
                    Message = "Not found this post.",
                    Success = false
                };
            }

            if (post.AccId != acc_id)
            {
                return new DeletePostResponseDTO
                {
                    Message = "You are not permission for this action.",
                    Success = false
                };
            }

            var isRestore = await _postRepository.ActivePost(request.PostId);

            if (!isRestore)
            {
                return new DeletePostResponseDTO
                {
                    Message = "Restore post fail.",
                    Success = false
                };
            }

            return new DeletePostResponseDTO
            {
                Message = "Restore post success.",
                Success = true
            };
        }

        /// <summary>
        ///     Soft delete a post - just update status
        /// </summary>
        public async Task<DeletePostResponseDTO?> TempDeleted(string? acc_id, DeletePostRequestDTO request)
        {
            if (acc_id == null || string.IsNullOrWhiteSpace(request.PostId)) return null;

            var post = await _postRepository.GetPostById(request.PostId);
            if (post == null)
                return new DeletePostResponseDTO { Message = "Not found this post", Success = false };

            if (post.AccId != acc_id)
                return new DeletePostResponseDTO { Message = "You are not permission for this action.", Success = false };

            // Nếu đã soft delete > 30 ngày thì không thao tác nữa
            if (post.DeletedAt.HasValue && (DateTime.UtcNow - post.DeletedAt.Value).TotalDays >= 30)
                return new DeletePostResponseDTO { Message = "This post has been soft deleted.", Success = false };

            // 1) Thực hiện xóa mềm trước
            var isSoftDelete = await _postRepository.InactivePost(post.PostId);
            await _sharePostRepository.DisableAsync(post.PostId);

            if (!isSoftDelete)
                return new DeletePostResponseDTO { Message = "Soft delete post fail.", Success = false };

            // 2) Chuẩn bị gửi thông báo, nhưng phải có người nhận
            var sharePosts = await _sharePostRepository.GetByPost(post.PostId) ?? new List<SharePost>();
            var receiverIds = sharePosts
                .Select(sp => sp.AccId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            try
            {
                if (receiverIds.Count > 0)
                {
                    var account = await _accountRepository.GetAccountByIdAsync(post.AccId);
                    var notiRequest = new SendNotificationRequestDTO
                    {
                        ReceiverIds = receiverIds,
                        SenderId = account?.AccId, // Nếu service yêu cầu bắt buộc, đảm bảo != null
                        CategoryNotiId = "6899d3cf963f2ffdfac460c8",
                        TargetId = post.PostId,
                        TargetType = "Post",
                        Content = $"{account?.FullName}'s post that you shared has been deleted."
                    };

                    var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);
                    if (!notiResponse.Success)
                    {
                        // Log nhẹ, không throw để tránh 500
                        Console.WriteLine($"Notification failed: {notiResponse.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Bảo vệ không để noti làm vỡ API
                Console.WriteLine($"Notification exception: {ex}");
            }

            return new DeletePostResponseDTO
            {
                Message = "Soft delete post success.",
                Success = true
            };
        }

        //public async Task<List<Post>> SearchPostsByKeyword(string keyword)
        //{
        //    return await _postRepository.SearchPostsByKeyword(keyword);
        //}

        //public async Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic)
        //{
        //    return await _postRepository.SearchPostsByCategories(categoryIds, isAndLogic);
        //}

        /// <summary>
        /// Searches for posts based on a keyword, category IDs, or both. 
        /// It allows searching with a keyword, categories, or a combination of both, 
        /// and ensures that at least one of them is provided. If both criteria are given, 
        /// the method returns the intersection of the results for the keyword and categories.
        /// </summary>
        /// <param name="keyword">The keyword to search for in the post content. This can be null or an empty string.</param>
        /// <param name="categoryIds">A list of category IDs to filter the posts by. This can be null or empty.</param>
        /// <param name="isAndLogic">A boolean value indicating whether to use AND logic (true) or OR logic (false) 
        /// for filtering posts based on category membership.</param>
        /// <returns>A list of posts that match the provided keyword and/or categories.</returns>
        public async Task<ListPostResponseDTO?> SearchPosts(string? keyword, List<string>? categoryIds, bool isAndLogic)
        {
            // Check if both keyword and categoryIds are empty or null
            if (string.IsNullOrWhiteSpace(keyword) && (categoryIds == null || categoryIds.Count == 0))
            {
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            List<Post> posts;

            // Case 1: Only keyword is provided
            if (!string.IsNullOrWhiteSpace(keyword) && (categoryIds == null || categoryIds.Count == 0))
            {
                // Perform search based on the keyword alone
                posts = await _postRepository.SearchPostsByKeyword(keyword);
            }
            // Case 2: Only categoryIds are provided
            else if (string.IsNullOrWhiteSpace(keyword) && categoryIds != null && categoryIds.Count > 0)
            {
                // Perform search based on categories alone
                posts = await _postRepository.SearchPostsByCategories(categoryIds, isAndLogic);
            }
            // Case 3: Both keyword and categoryIds are provided
            else
            {
                // Search posts by keyword first
                var postsByKeyword = await _postRepository.SearchPostsByKeyword(keyword);

                // Search posts by categories
                var postsByCategories = await _postRepository.SearchPostsByCategories(categoryIds, isAndLogic);

                // Get the intersection of the two result sets (posts that match both the keyword and categories)
                var postIdsByKeyword = new HashSet<string>(postsByKeyword.Select(p => p.PostId));
                posts = postsByCategories
                    .Where(p => postIdsByKeyword.Contains(p.PostId))
                    .ToList();
            }

            if (posts == null || posts.Count == 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "No post found!",
                    Success = true,
                    HasMore = false,
                    Data = null
                };
            }

            // Map dữ liệu liên quan
            List<PostMapper> data = new List<PostMapper>();
            foreach (var post in posts)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);
                var postMapper = new PostMapper
                {
                    ReactionCount = reactions.Count,
                    CommentCount = comments.Count,
                    ShareCount = shares?.Count,
                    Post = post,
                    OwnerPost = ownerPost,
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId)
                };

                data.Add(postMapper);
            }

            // Trả về kết quả
            return new ListPostResponseDTO
            {
                Message = "Get list post success.",
                Success = true,
                HasMore = false, // Không cần phân trang nên HasMore luôn là false
                Data = data
            };
        }

        public async Task<PostResponseDTO?> UpdatePost(string? username, UpdatePostRequestDTO? request)
        {
            //Kiem tra dau vao, PostId tu dong nen khong can kiem tra
            if (request == null)
                return null;

            if (username == null)
                return null;

            var ownAccount = await _accountRepository.GetAccountByUsername(username);
            if (ownAccount == null)
                return null;

            //1. Update post thong tin co ban
            if (request.PostId == null)
                return null;

            var post = await _postRepository.GetPostById(request.PostId);

            if (post == null)
                return null;

            post.PostContent = request.Content;
            post.PostScope = request.Privacy;
            post.UpdatedAt = DateTime.UtcNow;

            var newPost = await _postRepository.UpdatePost(post);

            if (newPost == null)
                return new PostResponseDTO
                {
                    Message = "Update post is fail.",
                    Success = false
                };

            //2. Update Post Category neu co
            //2.1 Xoa Post Category cu neu co yeu cau
            if (request.IsDeleteAllCategory == true)
            {
                await _postCategoryRepository.DeleteAllByPostId(request.PostId);
            }

            if (request.CategoriesToRemove != null && request.CategoriesToRemove.Count() > 0)
            {
                foreach (var categoryDelete in request.CategoriesToRemove)
                {
                    await _postCategoryRepository.DeletePostCategoryById(categoryDelete);
                }
            }

            //2.2 Them Post category neu co
            List<PostCategory> postCategories = new List<PostCategory>();

            if (request.CategoriesToAdd != null && request.CategoriesToAdd.Count() > 0)
            {
                foreach (var categoryAdd in request.CategoriesToAdd)
                {
                    var categoryById = await _categoryPostRepository.GetCategoryById(categoryAdd);
                    if (categoryById == null)
                        continue;

                    var postCategory = new PostCategory();
                    postCategory.CreatedAt = DateTime.UtcNow;
                    postCategory.PostId = newPost.PostId; //Lay post id cua post vua update xong cho chac
                    postCategory.CategoryId = categoryAdd;
                    postCategory.CategoryName = categoryById.CategoryName.ToString();

                    var newPostCategory = await _postCategoryRepository.CreatePostCategory(postCategory);

                    if (newPostCategory != null)
                        postCategories.Add(newPostCategory);
                }
            }

            //3. Post Image
            //3.1 Xóa những image của post trong ds yêu cầu xóa
            if (request.IsDeleteAllImage == true)
            {
                await _postImageRepository.DeleteAllByPostId(newPost.PostId);
            }

            if (request.ImagesToRemove != null && request.ImagesToRemove.Count() > 0)
            {
                foreach (var imagesToRemove in request.ImagesToRemove)
                {
                    var image = await _postImageRepository.GetPostImageById(imagesToRemove);

                    if (image == null) continue;

                    var isDeletedImage = await _postImageRepository.DeleteImageById(image.PostImageId);

                    if (isDeletedImage == true)
                    {
                        //Xóa image đó trên firebase
                        await _uploadFileService.DeleteFile(image.ImageUrl);
                    }
                }
            }

            //3.2 Add image mới nếu có
            List<PostImage> postImages = new List<PostImage>();

            if (request.ImagesToAdd != null && request.ImagesToAdd.Count > 0)
            {
                //Goi method upload List image tu Upload file service
                List<FileUploadResponseDTO> listImageUrl = await _uploadFileService.UploadListImage(request.ImagesToAdd);

                if (listImageUrl != null && listImageUrl.Count > 0)
                {
                    foreach (var image in listImageUrl)
                    {
                        var postImage = new PostImage();
                        postImage.PostId = newPost.PostId;
                        postImage.ImageUrl = image.UrlFile ?? "";

                        var newPostImage = await _postImageRepository.CreatePostImage(postImage);

                        if (newPostImage != null)
                            postImages.Add(newPostImage);
                    }
                }
            }

            //4. Hashtag
            //4.1 Xóa những hashtag trong list cần xóa
            if (request.IsDeleteAllHashtag == true)
            {
                await _hashTagRepository.DeleteAllByPostId(newPost.PostId);
            }

            if (request.HashTagToRemove != null && request.HashTagToRemove.Count > 0)
            {
                foreach (var hashtagToRemove in request.HashTagToRemove)
                {
                    await _hashTagRepository.DeleteHashTagById(hashtagToRemove);
                }
            }

            //4.2 Add thêm hashtag mới
            List<HashTag> hashTags = new List<HashTag>();

            if (request.HashTagToAdd != null && request.HashTagToAdd.Count > 0)
            {
                foreach (var itemHashtag in request.HashTagToAdd)
                {
                    var hashtag = new HashTag();
                    hashtag.HashTagContent = itemHashtag;
                    hashtag.PostId = newPost.PostId;
                    hashtag.CreateAt = DateTime.UtcNow;

                    var newHashTag = await _hashTagRepository.CreateHashTag(hashtag);

                    if (newHashTag != null)
                        hashTags.Add(newHashTag);
                }
            }

            //5. Post Tag
            //5.1 Xóa Post Tag cũ
            if (request.IsDeleteAllFriend == true)
            {
                await _postTagRepository.DeleteAllByPostId(newPost.PostId);
            }

            if (request.PostTagsToRemove != null && request.PostTagsToRemove.Count > 0)
            {
                foreach (var postTagToRemove in request.PostTagsToRemove)
                {
                    await _postTagRepository.DeletePostTagById(postTagToRemove);
                }
            }

            //5.2 Thêm Post Tag mới
            List<PostTag> postTags = new List<PostTag>();

            if (request.PostTagsToAdd != null && request.PostTagsToAdd.Count > 0)
            {
                foreach (var friendId in request.PostTagsToAdd)
                {
                    var account = await _accountRepository.GetAccountById(friendId);
                    if (account == null)
                        continue;

                    var postTag = new PostTag();
                    postTag.AccId = account.AccId;
                    postTag.Fullname = account.FullName;
                    postTag.Username = account.Username;
                    postTag.PostId = newPost.PostId;
                    postTag.CreatedAt = DateTime.UtcNow;

                    var newPostTag = await _postTagRepository.CreatePostTag(postTag);
                    if (newPostTag != null)
                        postTags.Add(newPostTag);
                }
            }

            //FINAL: Create data to response
            PostMapper data = new PostMapper();
            data.Post = newPost;
            data.PostTags = postTags;
            data.PostCategories = postCategories;
            data.PostImages = postImages;
            data.HashTags = hashTags;

            return new PostResponseDTO
            {
                Message = "Update post is successfully.",
                Success = true,
                Data = data
            };
        }

        public async Task<List<Post>> SearchPostsInGroupAsync(string groupId, string keyword)
        {
            return await _postRepository.SearchPostsInGroupAsync(groupId, keyword);
        }

        public async Task<ListPostInGroupResponseDTO> SearchPostsWithAccountAsync(string groupId, string keyword)
        {
            

            if (groupId == null)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "User has not joined any groups.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 2. Lấy list post thuộc các groupId đó

            var posts = await _postRepository.SearchPostsInGroupAsync(groupId, keyword);
            if (posts == null || posts.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

           

            // 3. Chuẩn bị dữ liệu trả về
            var data = new List<PostInGroupMapper>();
            foreach (var post in posts)
            {
                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);
                var postMapper = new PostInGroupMapper
                {
                    Post = post,
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                    ReactionCount = (await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post")).Count,
                    CommentCount = (await _commentRepository.GetAllByPost(post.PostId)).Count,
                    ShareCount = (await _sharePostRepository.GetByPost(post.PostId))?.Count ?? 0,
                    OwnerPost = ownerPost,

                };

                // 4. Lấy Group cho post
                if (!string.IsNullOrEmpty(post.GroupId))
                {
                    postMapper.Group = await _groupRepository.GetGroupById(post.GroupId);
                }

                data.Add(postMapper);
            }

            return new ListPostInGroupResponseDTO
            {
                Message = "Get posts from user's groups successful.",
                Success = true,
                HasMore = false,
                Count = data.Count,
                Data = data
            };

        }

        public async Task<ListPostResponseDTO?> GetListPostValid()
        {
            //1. Lấy list post valid
            var listPostValid = await _postRepository.GetListPost(0);

            if (listPostValid == null)
                return null;

            if (listPostValid.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostValid)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var postMapper = new PostMapper();
                postMapper.Post = post;

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

                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

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

        public async Task<ListPostResponseDTO?> GetListPostDeleted()
        {
            //1. Lấy list post invalid
            var listPostValid = await _postRepository.GetListPost(1);

            if (listPostValid == null)
                return null;

            if (listPostValid.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostValid)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);

                var postMapper = new PostMapper();
                postMapper.Post = post;
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

                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

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

        // PostService - Updated GetListDeletedPostByAccount với SharePost
        public async Task<ListPostResponseDTO?> GetListDeletedPostAndShareByAccount(string? accId)
        {
            if (accId == null)
                return null;

            // 1. Lấy cả deleted Posts và deleted SharePosts
            var deletedPosts = await _postRepository.GetDeletedByAccId(accId);
            var deletedSharePosts = await _sharePostRepository.GetDeletedByAccId(accId);

            // 2. Tạo danh sách kết hợp
            var combinedItems = new List<BasePostItem>();

            // Thêm deleted Posts
            if (deletedPosts != null)
            {
                foreach (var post in deletedPosts)
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = post.PostId,
                        CreatedAt = post.CreatedAt ?? DateTime.MinValue,
                        Type = "Post",
                        Post = post
                    });
                }
            }

            // Thêm deleted SharePosts
            if (deletedSharePosts != null)
            {
                foreach (var sharePost in deletedSharePosts)
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = sharePost.SharePostId,
                        CreatedAt = sharePost.CreatedAt,
                        Type = "SharePost",
                        SharePost = sharePost
                    });
                }
            }

            // 3. Sắp xếp theo CreatedAt giảm dần
            var sortedItems = combinedItems
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (sortedItems.Count <= 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "List deleted post is empty.",
                    Success = false,
                    Count = 0,
                    Data = null
                };
            }

            // 4. Map dữ liệu liên quan
            List<PostMapper> data = new List<PostMapper>();

            foreach (var item in sortedItems)
            {
                if (item.Type == "Post" && item.Post != null)
                {
                    // Xử lý deleted Post
                    var post = item.Post;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                    var comments = await _commentRepository.GetAllByPost(post.PostId);
                    var shares = await _sharePostRepository.GetByPost(post.PostId);

                    var account = await _accountRepository.GetAccountById(post.AccId);
                    var ownerPost = _mapper.Map<MyProfileDTO>(account);

                    var postMapper = new PostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        Post = post,
                        OwnerPost = ownerPost,
                        PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                        HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                        PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                        PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                        ItemType = "Post"
                    };

                    data.Add(postMapper);
                }
                else if (item.Type == "SharePost" && item.SharePost != null)
                {
                    // Xử lý deleted SharePost
                    var sharePost = item.SharePost;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(sharePost.SharePostId, "SharePost");
                    var comments = await _commentRepository.GetAllByPost(sharePost.SharePostId);
                    var shares = await _sharePostRepository.GetByPost(sharePost.SharePostId);

                    var account = await _accountRepository.GetAccountById(sharePost.AccId);
                    var ownerSharePost = _mapper.Map<MyProfileDTO>(account);

                    // Lấy thông tin post gốc
                    var originalPost = await _postRepository.GetPostById(sharePost.PostId);
                    PostMapper? originalPostMapper = null;

                    if (originalPost != null)
                    {
                        var originalAccount = await _accountRepository.GetAccountById(originalPost.AccId);
                        var originalOwner = _mapper.Map<MyProfileDTO>(originalAccount);

                        var originalReactions = await _reactionRepository.GetAllByEntityAsync(originalPost.PostId, "Post");
                        var originalComments = await _commentRepository.GetAllByPost(originalPost.PostId);
                        var originalShares = await _sharePostRepository.GetByPost(originalPost.PostId);

                        originalPostMapper = new PostMapper
                        {
                            ReactionCount = originalReactions.Count,
                            CommentCount = originalComments.Count,
                            ShareCount = originalShares?.Count,
                            Post = originalPost,
                            OwnerPost = originalOwner,
                            PostImages = await _postImageRepository.GetPostImageByPost(originalPost.PostId),
                            HashTags = await _hashTagRepository.GetHashTagByPost(originalPost.PostId),
                            PostCategories = await _postCategoryRepository.GetCategoryByPost(originalPost.PostId),
                            PostTags = await _postTagRepository.GetPostTagByPost(originalPost.PostId)
                        };
                    }

                    var sharePostMapper = new SharePostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        SharePost = sharePost,
                        OwnerSharePost = ownerSharePost,
                        OriginalPost = originalPostMapper,
                        HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                        SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId)
                    };

                    var postMapper = new PostMapper
                    {
                        ItemType = "SharePost",
                        SharePostData = sharePostMapper
                    };

                    data.Add(postMapper);
                }
            }

            return new ListPostResponseDTO
            {
                Message = "Get list deleted post and sharepost success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }

        public async Task<ListPostResponseDTO?> GetListDeletedPostByAccount(string? accId)
        {
            if (accId == null)
                return null;

            var deletedPosts = await _postRepository.GetDeletedByAccId(accId);
            if (deletedPosts == null)
                return null;

            if (deletedPosts.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in deletedPosts)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);

                var postMapper = new PostMapper();
                postMapper.Post = post;
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

                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

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
        public async Task<ListPostResponseDTO?> GetListAllPost()
        {
            //1. Lấy list post valid
            var listPostValid = await _postRepository.GetListPost(-1);

            if (listPostValid == null)
                return null;

            if (listPostValid.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostValid)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var postMapper = new PostMapper();
                postMapper.Post = post;

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
                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

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

        public async Task<ListPostResponseDTO?> GetListInfinitePost(string? last_post_id, int page_size)
        {
            // 1. Lấy danh sách post theo phân trang (infinite scroll)
            var (posts, hasMore) = await _postRepository.GetPaginatedPosts(last_post_id, page_size);

            if (posts == null || posts.Count == 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 2. Map dữ liệu liên quan
            List<PostMapper> data = new List<PostMapper>();
            foreach (var post in posts)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);
                var postMapper = new PostMapper
                {

                    ReactionCount = reactions.Count,
                    CommentCount = comments.Count,
                    ShareCount = shares?.Count,
                    Post = post,
                    OwnerPost = ownerPost,
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId)
                };

                data.Add(postMapper);
            }

            // 3. Trả về kết quả
            return new ListPostResponseDTO
            {
                Message = "Get list post success.",
                Success = true,
                HasMore = hasMore,
                Data = data
            };
        }

        public async Task<ListPostResponseDTO?> GetListInfinitePostAndSharePost(string? lastPostId, string? lastSharePostId, int pageSize)
        {
            // 1. Lấy danh sách post và sharepost theo phân trang
            var (items, hasMore) = await _postRepository.GetPaginatedPostsAndSharePosts(lastPostId, lastSharePostId, pageSize);

            if (items == null || items.Count == 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Count = 0,
                    Data = null
                };
            }

            // 2. Map dữ liệu liên quan
            List<PostMapper> data = new List<PostMapper>();

            foreach (var item in items)
            {
                if (item.Type == "Post" && item.Post != null)
                {
                    // Xử lý Post
                    var post = item.Post;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                    var comments = await _commentRepository.GetAllByPost(post.PostId);
                    var shares = await _sharePostRepository.GetByPost(post.PostId);

                    var account = await _accountRepository.GetAccountById(post.AccId);
                    var ownerPost = _mapper.Map<MyProfileDTO>(account);

                    var postMapper = new PostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        Post = post,
                        OwnerPost = ownerPost,
                        PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                        HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                        PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                        PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                        ItemType = "Post"
                    };

                    data.Add(postMapper);
                }
                else if (item.Type == "SharePost" && item.SharePost != null)
                {
                    // Xử lý SharePost
                    var sharePost = item.SharePost;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(sharePost.SharePostId, "SharePost");
                    var comments = await _commentRepository.GetAllByPost(sharePost.SharePostId);
                    var shares = await _sharePostRepository.GetByPost(sharePost.SharePostId);

                    var account = await _accountRepository.GetAccountById(sharePost.AccId);
                    var ownerSharePost = _mapper.Map<MyProfileDTO>(account);

                    // Lấy thông tin post gốc
                    var originalPost = await _postRepository.GetPostById(sharePost.PostId);
                    PostMapper? originalPostMapper = null;

                    if (originalPost != null)
                    {
                        var originalAccount = await _accountRepository.GetAccountById(originalPost.AccId);
                        var originalOwner = _mapper.Map<MyProfileDTO>(originalAccount);

                        var originalReactions = await _reactionRepository.GetAllByEntityAsync(originalPost.PostId, "Post");
                        var originalComments = await _commentRepository.GetAllByPost(originalPost.PostId);
                        var originalShares = await _sharePostRepository.GetByPost(originalPost.PostId);

                        originalPostMapper = new PostMapper
                        {
                            ReactionCount = originalReactions.Count,
                            CommentCount = originalComments.Count,
                            ShareCount = originalShares?.Count,
                            Post = originalPost,
                            OwnerPost = originalOwner,
                            PostImages = await _postImageRepository.GetPostImageByPost(originalPost.PostId),
                            HashTags = await _hashTagRepository.GetHashTagByPost(originalPost.PostId),
                            PostCategories = await _postCategoryRepository.GetCategoryByPost(originalPost.PostId),
                            PostTags = await _postTagRepository.GetPostTagByPost(originalPost.PostId)
                        };
                    }

                    var sharePostTag = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId);

                    var sharePostMapper = new SharePostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        SharePost = sharePost,
                        OwnerSharePost = ownerSharePost,
                        OriginalPost = originalPostMapper,
                        HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                        SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId)
                    };

                    var postMapper = new PostMapper
                    {
                        ItemType = "SharePost",
                        SharePostData = sharePostMapper
                    };

                    data.Add(postMapper);
                }
            }

            // 3. Trả về kết quả
            return new ListPostResponseDTO
            {
                Message = "Get list post and sharepost success.",
                Success = true,
                HasMore = hasMore,
                Count = data.Count,
                Data = data
            };
        }

        public async Task<ListPostResponseDTO?> GetListPostCheckedAI()
        {
            //1. Lấy list post valid
            var listPostValid = await _postRepository.GetListPostCheckedByAI();

            if (listPostValid == null)
                return null;

            if (listPostValid.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostValid)
            {
                var postMapper = new PostMapper();
                postMapper.Post = post;

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
                var owner = await _accountRepository.GetAccountByAccId(post.AccId);
                var ownerSharePost = _mapper.Map<MyProfileDTO>(owner);
                if (owner != null) {
                    postMapper.OwnerPost = ownerSharePost;
                }

                //Add post mappaer vào List post mapper
                data.Add(postMapper);
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post valid is success.",
                Success = true,
                Data = data
            };
        }
        public async Task<ListPostResponseDTO?> GetAllPostsForAdmin()
        {
            //1. Lấy list post valid
            var listPostValid = await _postRepository.GetAllPostsForAdmin();

            if (listPostValid == null)
                return null;

            if (listPostValid.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostValid)
            {
                var postMapper = new PostMapper();
                postMapper.Post = post;

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
                var owner = await _accountRepository.GetAccountByAccId(post.AccId);
                var ownerSharePost = _mapper.Map<MyProfileDTO>(owner);
                if (owner != null)
                {
                    postMapper.OwnerPost = ownerSharePost;
                }

                //Add post mappaer vào List post mapper
                data.Add(postMapper);
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post valid is success.",
                Success = true,
                Data = data
            };
        }

        public async Task<bool?> CheckPostByAI(string postId)
        {
            if (string.IsNullOrEmpty(postId)) return null;
            var post = await _postRepository.GetPostById(postId);

            //var check = await _cohereService.IsAgricultureRelatedAsync(post.PostContent);
            
            //if (check == false)
            //{
            //    post.Status = 1;
            //    var update = await _postRepository.UpdatePost(post);
            //    return false;
            //}
            //else
            //{
                post.Status = 0;
                var update = await _postRepository.UpdatePost(post);
            return update != null;

            //}

        }

        public async Task<ListPostResponseDTO?> GetPostsOwner(string? accId)
        {
            if (accId == null)
                return null;

            var listPostOwner = await _postRepository.GetListPostByAccId(accId, null);

            if (listPostOwner == null)
                return null;

            if (listPostOwner.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostOwner)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);

                var postMapper = new PostMapper();
                postMapper.Post = post;
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

                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

                //Add post mappaer vào List post mapper
                data.Add(postMapper);
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post is success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }

        public async Task<ListPostResponseDTO?> GetPostsOwnerWithShare(string? accId)
        {
            if (accId == null)
                return null;

            // 1. Lấy cả Posts và SharePosts của user
            var listPosts = await _postRepository.GetByAccId(accId);
            var listSharePosts = await _sharePostRepository.GetByAccId(accId);

            // 2. Tạo danh sách kết hợp
            var combinedItems = new List<BasePostItem>();

            // Thêm Posts (lọc isDeleted = false)
            if (listPosts != null)
            {
                foreach (var post in listPosts.Where(p => !p.IsDeleted))
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = post.PostId,
                        CreatedAt = post.CreatedAt ?? DateTime.MinValue,
                        Type = "Post",
                        Post = post
                    });
                }
            }

            // Thêm SharePosts (lọc isDeleted = false)
            if (listSharePosts != null)
            {
                foreach (var sharePost in listSharePosts.Where(sp => !sp.IsDeleted))
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = sharePost.SharePostId,
                        CreatedAt = sharePost.CreatedAt,
                        Type = "SharePost",
                        SharePost = sharePost
                    });
                }
            }

            // 3. Sắp xếp theo CreatedAt giảm dần
            var sortedItems = combinedItems
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (sortedItems.Count <= 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    Count = 0,
                    Data = null
                };
            }

            // 4. Map dữ liệu liên quan
            List<PostMapper> data = new List<PostMapper>();

            foreach (var item in sortedItems)
            {
                if (item.Type == "Post" && item.Post != null)
                {
                    // Xử lý Post - tương tự như GetListInfinitePostAndSharePost
                    var post = item.Post;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                    var comments = await _commentRepository.GetAllByPost(post.PostId);
                    var shares = await _sharePostRepository.GetByPost(post.PostId);

                    var account = await _accountRepository.GetAccountById(post.AccId);
                    var ownerPost = _mapper.Map<MyProfileDTO>(account);

                    var postMapper = new PostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        Post = post,
                        OwnerPost = ownerPost,
                        PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                        HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                        PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                        PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                        ItemType = "Post"
                    };

                    data.Add(postMapper);
                }
                else if (item.Type == "SharePost" && item.SharePost != null)
                {
                    // Xử lý SharePost - tương tự như GetListInfinitePostAndSharePost
                    var sharePost = item.SharePost;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(sharePost.SharePostId, "SharePost");
                    var comments = await _commentRepository.GetAllByPost(sharePost.SharePostId);
                    var shares = await _sharePostRepository.GetByPost(sharePost.SharePostId);

                    var account = await _accountRepository.GetAccountById(sharePost.AccId);
                    var ownerSharePost = _mapper.Map<MyProfileDTO>(account);

                    // Lấy thông tin post gốc
                    var originalPost = await _postRepository.GetPostById(sharePost.PostId);
                    PostMapper? originalPostMapper = null;

                    if (originalPost != null)
                    {
                        var originalAccount = await _accountRepository.GetAccountById(originalPost.AccId);
                        var originalOwner = _mapper.Map<MyProfileDTO>(originalAccount);

                        var originalReactions = await _reactionRepository.GetAllByEntityAsync(originalPost.PostId, "Post");
                        var originalComments = await _commentRepository.GetAllByPost(originalPost.PostId);
                        var originalShares = await _sharePostRepository.GetByPost(originalPost.PostId);

                        originalPostMapper = new PostMapper
                        {
                            ReactionCount = originalReactions.Count,
                            CommentCount = originalComments.Count,
                            ShareCount = originalShares?.Count,
                            Post = originalPost,
                            OwnerPost = originalOwner,
                            PostImages = await _postImageRepository.GetPostImageByPost(originalPost.PostId),
                            HashTags = await _hashTagRepository.GetHashTagByPost(originalPost.PostId),
                            PostCategories = await _postCategoryRepository.GetCategoryByPost(originalPost.PostId),
                            PostTags = await _postTagRepository.GetPostTagByPost(originalPost.PostId)
                        };
                    }

                    var sharePostMapper = new SharePostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        SharePost = sharePost,
                        OwnerSharePost = ownerSharePost,
                        OriginalPost = originalPostMapper,
                        HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                        SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId)
                    };

                    var postMapper = new PostMapper
                    {
                        ItemType = "SharePost",
                        SharePostData = sharePostMapper
                    };

                    data.Add(postMapper);
                }
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post and sharepost success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }
        public async Task<ListPostResponseDTO?> GetPostsPublicByAccId(string? accId)
        {
            if (accId == null)
                return null;

            var listPostOwner = await _postRepository.GetListPostByAccId(accId, "Public");

            if (listPostOwner == null)
                return null;

            if (listPostOwner.Count <= 0)
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false
                };

            //2. Lấy các thành phần cho từng post
            List<PostMapper> data = new List<PostMapper>();

            foreach (var post in listPostOwner)
            {
                var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await _commentRepository.GetAllByPost(post.PostId);
                var shares = await _sharePostRepository.GetByPost(post.PostId);

                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);

                var postMapper = new PostMapper();
                postMapper.Post = post;
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

                postMapper.ReactionCount = reactions.Count;
                postMapper.CommentCount = comments.Count;
                postMapper.ShareCount = shares?.Count;

                //Add post mappaer vào List post mapper
                data.Add(postMapper);
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post is success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }

        public async Task<ListPostResponseDTO?> GetPostsPublicWithShareByAccId(string? accId)
        {
            if (accId == null)
                return null;

            // 1. Lấy cả Posts và SharePosts của user (chỉ Public)
            var listPosts = await _postRepository.GetByAccId(accId);
            var listSharePosts = await _sharePostRepository.GetByAccId(accId);

            // 2. Tạo danh sách kết hợp
            var combinedItems = new List<BasePostItem>();

            // Thêm Posts (lọc Public, isDeleted = false, isInGroup = false)
            if (listPosts != null)
            {
                foreach (var post in listPosts.Where(p => (p.IsDeleted == false) && p.PostScope == "Public" && (p.IsInGroup == false)))
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = post.PostId,
                        CreatedAt = post.CreatedAt ?? DateTime.MinValue,
                        Type = "Post",
                        Post = post
                    });
                }

            }

            // Thêm SharePosts (lọc Public, isDeleted = false)
            if (listSharePosts != null)
            {
                foreach (var sharePost in listSharePosts.Where(sp => !sp.IsDeleted && sp.SharePostScope == "Public"))
                {
                    combinedItems.Add(new BasePostItem
                    {
                        Id = sharePost.SharePostId,
                        CreatedAt = sharePost.CreatedAt,
                        Type = "SharePost",
                        SharePost = sharePost
                    });
                }
            }

            // 3. Sắp xếp theo CreatedAt giảm dần
            var sortedItems = combinedItems
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (sortedItems.Count <= 0)
            {
                return new ListPostResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    Count = 0,
                    Data = null
                };
            }

            // 4. Map dữ liệu liên quan (tương tự như GetPostsOwnerWithShare)
            List<PostMapper> data = new List<PostMapper>();

            foreach (var item in sortedItems)
            {
                if (item.Type == "Post" && item.Post != null)
                {
                    // Xử lý Post
                    var post = item.Post;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post");
                    var comments = await _commentRepository.GetAllByPost(post.PostId);
                    var shares = await _sharePostRepository.GetByPost(post.PostId);

                    var account = await _accountRepository.GetAccountById(post.AccId);
                    var ownerPost = _mapper.Map<MyProfileDTO>(account);

                    var postMapper = new PostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        Post = post,
                        OwnerPost = ownerPost,
                        PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                        HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                        PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                        PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                        ItemType = "Post"
                    };

                    data.Add(postMapper);
                }
                else if (item.Type == "SharePost" && item.SharePost != null)
                {
                    // Xử lý SharePost
                    var sharePost = item.SharePost;
                    var reactions = await _reactionRepository.GetAllByEntityAsync(sharePost.SharePostId, "SharePost");
                    var comments = await _commentRepository.GetAllByPost(sharePost.SharePostId);
                    var shares = await _sharePostRepository.GetByPost(sharePost.SharePostId);

                    var account = await _accountRepository.GetAccountById(sharePost.AccId);
                    var ownerSharePost = _mapper.Map<MyProfileDTO>(account);

                    // Lấy thông tin post gốc
                    var originalPost = await _postRepository.GetPostById(sharePost.PostId);
                    PostMapper? originalPostMapper = null;

                    if (originalPost != null)
                    {
                        var originalAccount = await _accountRepository.GetAccountById(originalPost.AccId);
                        var originalOwner = _mapper.Map<MyProfileDTO>(originalAccount);

                        var originalReactions = await _reactionRepository.GetAllByEntityAsync(originalPost.PostId, "Post");
                        var originalComments = await _commentRepository.GetAllByPost(originalPost.PostId);
                        var originalShares = await _sharePostRepository.GetByPost(originalPost.PostId);

                        originalPostMapper = new PostMapper
                        {
                            ReactionCount = originalReactions.Count,
                            CommentCount = originalComments.Count,
                            ShareCount = originalShares?.Count,
                            Post = originalPost,
                            OwnerPost = originalOwner,
                            PostImages = await _postImageRepository.GetPostImageByPost(originalPost.PostId),
                            HashTags = await _hashTagRepository.GetHashTagByPost(originalPost.PostId),
                            PostCategories = await _postCategoryRepository.GetCategoryByPost(originalPost.PostId),
                            PostTags = await _postTagRepository.GetPostTagByPost(originalPost.PostId)
                        };
                    }

                    var sharePostMapper = new SharePostMapper
                    {
                        ReactionCount = reactions.Count,
                        CommentCount = comments.Count,
                        ShareCount = shares?.Count,
                        SharePost = sharePost,
                        OwnerSharePost = ownerSharePost,
                        OriginalPost = originalPostMapper,
                        HashTags = await _hashTagRepository.GetHashTagByPost(sharePost.SharePostId),
                        SharePostTags = await _sharePostTagRepository.GetAllBySharePost(sharePost.SharePostId)
                    };

                    var postMapper = new PostMapper
                    {
                        ItemType = "SharePost",
                        SharePostData = sharePostMapper
                    };

                    data.Add(postMapper);
                }
            }

            return new ListPostResponseDTO
            {
                Message = "Get list post and sharepost success.",
                Success = true,
                Count = data.Count,
                Data = data
            };
        }

        /// <summary>
        /// get list post in list group of user
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        public async Task<ListPostInGroupResponseDTO?> GetPostsInYourGroups(string? last_post_id, int page_size,string accId)
        {
            // 1. Lấy danh sách groupId user đã tham gia
            var groupIds = await _groupRepository.GetGroupIdsByUserId(accId);
            if (groupIds == null || groupIds.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "User has not joined any groups.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 2. Lấy list post thuộc các groupId đó
           
            var (posts, hasMore) = await _postRepository.GetListPostInYourGroup(last_post_id, page_size, groupIds);
            if (posts == null || posts.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            var listPostInGroup = posts;

            if (listPostInGroup.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "No posts found in user's groups.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 3. Chuẩn bị dữ liệu trả về
            var data = new List<PostInGroupMapper>();
            foreach (var post in listPostInGroup)
            {
                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);
                var postMapper = new PostInGroupMapper
                {
                    Post = post,
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                    ReactionCount = (await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post")).Count,
                    CommentCount = (await _commentRepository.GetAllByPost(post.PostId)).Count,
                    ShareCount = (await _sharePostRepository.GetByPost(post.PostId))?.Count ?? 0,
                    OwnerPost = ownerPost,

                };

                // 4. Lấy Group cho post
                if (!string.IsNullOrEmpty(post.GroupId))
                {
                    postMapper.Group = await _groupRepository.GetGroupById(post.GroupId);
                }

                data.Add(postMapper);
            }

            return new ListPostInGroupResponseDTO
            {
                Message = "Get posts from user's groups successful.",
                Success = true,
                HasMore = hasMore,
                Count = data.Count,
                Data = data
            };
        }
        /// <summary>
        /// get list post in list group detail
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        public async Task<ListPostInGroupResponseDTO?> GetPostsInGroupDetail(string? last_post_id, int page_size, string groupIds)
        {
           
            if (groupIds == null)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "User has not joined any groups.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 2. Lấy list post thuộc các groupId đó

            var (posts, hasMore) = await _postRepository.GetListPostInYourGroupDetail(last_post_id, page_size, groupIds);
            if (posts == null || posts.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "List post is empty.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            var listPostInGroup = posts;

            if (listPostInGroup.Count == 0)
            {
                return new ListPostInGroupResponseDTO
                {
                    Message = "No posts found in user's groups.",
                    Success = false,
                    HasMore = false,
                    Data = null
                };
            }

            // 3. Chuẩn bị dữ liệu trả về
            var data = new List<PostInGroupMapper>();
            foreach (var post in listPostInGroup)
            {
                var account = await _accountRepository.GetAccountById(post.AccId);
                var ownerPost = _mapper.Map<MyProfileDTO>(account);
                var postMapper = new PostInGroupMapper
                {
                    Post = post,
                    PostImages = await _postImageRepository.GetPostImageByPost(post.PostId),
                    HashTags = await _hashTagRepository.GetHashTagByPost(post.PostId),
                    PostCategories = await _postCategoryRepository.GetCategoryByPost(post.PostId),
                    PostTags = await _postTagRepository.GetPostTagByPost(post.PostId),
                    ReactionCount = (await _reactionRepository.GetAllByEntityAsync(post.PostId, "Post")).Count,
                    CommentCount = (await _commentRepository.GetAllByPost(post.PostId)).Count,
                    ShareCount = (await _sharePostRepository.GetByPost(post.PostId))?.Count ?? 0,
                    OwnerPost = ownerPost,

                };

                // 4. Lấy Group cho post
                if (!string.IsNullOrEmpty(post.GroupId))
                {
                    postMapper.Group = await _groupRepository.GetGroupById(post.GroupId);
                }

                data.Add(postMapper);
            }

            return new ListPostInGroupResponseDTO
            {
                Message = "Get posts from user's groups successful.",
                Success = true,
                HasMore = hasMore,
                Count = data.Count,
                Data = data
            };
        }
        public async Task<long> CountPublicPostsInGroupAsync(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return 0;
            return await _postRepository.CountPublicPostsInGroupAsync(groupId);
        }

        public async Task<List<string>> GetAllImage(string accId)
        {
            return await _postImageRepository.GetAllImage(accId);
        }

    }
}
