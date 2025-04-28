using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public PostService(IPostRepository postRepository, IPostCategoryRepository postCategoryRepository, IPostImageRepository postImageRepository, IHashTagRepository hashTagRepository, IPostTagRepository postTagRepository, ICategoryPostRepository categoryPostRepository, IUploadFileService uploadFileService, IAccountRepository accountRepository)
        {
            _postRepository = postRepository;
            _postCategoryRepository = postCategoryRepository;
            _postImageRepository = postImageRepository;
            _hashTagRepository = hashTagRepository;
            _postTagRepository = postTagRepository;
            _categoryPostRepository = categoryPostRepository;
            _uploadFileService = uploadFileService;
            _accountRepository = accountRepository;
        }

        /// <summary>
        ///     Add new post
        /// </summary>
        public async Task<CreatePostResponseDTO?> AddPost(string? username, CreatePostRequestDTO? request)
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

            var postRequest = new Post();
            postRequest.PostContent = request.PostContent;
            postRequest.PostScope = request.Privacy;
            postRequest.AccId = ownAccount.AccId;
            postRequest.CreatedAt = DateTime.UtcNow;

            var newPost = await _postRepository.CreatePost(postRequest);

            if (newPost == null)
                return new CreatePostResponseDTO
                {
                    Message = "Create post is fail.",
                    Success = false
                };

            //2. Add Post Category
            List<PostCategory> postCategories = new List<PostCategory>();

            if(request.ListCategoryOfPost != null && request.ListCategoryOfPost.Count > 0)
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
            
            if(request.ListImage != null &&  request.ListImage.Count > 0)
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

                        if(newPostImage != null) 
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
                    
                    if(newHashTag != null) 
                        hashTags.Add(newHashTag);
                }
            }

            //5. Add Post tag
            List<PostTag> postTags = new List<PostTag>();

            if(request.ListTagFriend != null && request.ListTagFriend.Count > 0)
            {
                foreach (var friendId in request.ListTagFriend)
                {
                    var account = await _accountRepository.GetAccountById(friendId);
                    if (account == null) 
                        continue;

                    var postTag = new PostTag();
                    postTag.AccId = account.AccId;
                    postTag.Username = account.Username;
                    postTag.PostId = newPost.PostId;
                    postTag.CreatedAt = DateTime.UtcNow;

                    var newPostTag = await _postTagRepository.CreatePostTag(postTag);
                    if (newPostTag != null)
                        postTags.Add(newPostTag);                    
                }
            }

            //Tạo data trả về
            PostMapper data = new PostMapper();
            data.Post = newPost;
            data.PostTags = postTags;
            data.PostCategories = postCategories;
            data.PostImages = postImages;
            data.HashTags = hashTags;

            return new CreatePostResponseDTO
            {
                Message = "Create post is successfully.",
                Success = true,
                Data = data
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
        /// <exception cref="ArgumentException">Thrown if neither the keyword nor categoryIds is provided for the search.</exception>
        public async Task<List<Post>> SearchPosts(string? keyword, List<string>? categoryIds, bool isAndLogic)
        {
            // Check if both keyword and categoryIds are empty or null
            if (string.IsNullOrWhiteSpace(keyword) && (categoryIds == null || categoryIds.Count == 0))
            {
                // If neither is provided, throw an exception
                throw new ArgumentException("At least one keyword or category must be provided for the search.");
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

            // Return the final list of posts
            return posts;
        }

        public async Task<UpdatePostResponseDTO?> UpdatePost(string? username, UpdatePostRequestDTO? request)
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
            if(request.PostId == null) 
                return null;

            var post = await _postRepository.GetPostById(request.PostId);

            if (post == null) 
                return null;

            post.PostContent = request.Content;
            post.PostScope = request.Privacy;
            post.UpdatedAt = DateTime.UtcNow;

            var newPost = await _postRepository.UpdatePost(post);

            if (newPost == null) 
                return new UpdatePostResponseDTO
                {
                    Message = "Update post is fail.",
                    Success = false
                };

            //2. Update Post Category neu co
            //2.1 Xoa Post Category cu neu co yeu cau
            if(request.IsDeleteAllCategory == true)
            {
                await _postCategoryRepository.DeleteAllByPostId(request.PostId);
            }

            if(request.CategoriesToRemove != null && request.CategoriesToRemove.Count() > 0)
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
                foreach(var categoryAdd in request.CategoriesToAdd)
                {
                    var categoryById = await _categoryPostRepository.GetCategoryById(categoryAdd);
                    if (categoryById == null) 
                        continue;

                    var postCategory = new PostCategory();
                    postCategory.CreatedAt = DateTime.UtcNow;
                    postCategory.PostId = newPost.PostId; //Lay post id cua post vua update xong cho chac
                    postCategory.CategoryId = categoryAdd;
                    postCategory.CategoryName = categoryById.CategoryName.ToString();

                    var newPostCategory =  await _postCategoryRepository.CreatePostCategory(postCategory);

                    if(newPostCategory != null)
                        postCategories.Add(newPostCategory);
                }
            }

            //3. Post Image
            //3.1 Xóa những image của post trong ds yêu cầu xóa
            if(request.IsDeleteAllImage == true)
            {
                await _postImageRepository.DeleteAllByPostId(newPost.PostId);
            }

            if(request.ImagesToRemove != null && request.ImagesToRemove.Count() > 0)
            {
                foreach(var imagesToRemove in request.ImagesToRemove)
                {
                    var image = await _postImageRepository.GetPostImageById(imagesToRemove);

                    if(image == null) continue;

                    var isDeletedImage = await _postImageRepository.DeleteImageById(image.PostImageId);

                    if(isDeletedImage == true)
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
            if(request.IsDeleteAllHashtag == true)
            {
                await _hashTagRepository.DeleteAllByPostId(newPost.PostId);
            }

            if(request.HashTagToRemove != null && request.HashTagToRemove.Count > 0)
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

            return new UpdatePostResponseDTO
            {
                Message = "Update post is successfully.",
                Success = true,
                Data = data
            };
        }
    }
}
