using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
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

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
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

    }
}
