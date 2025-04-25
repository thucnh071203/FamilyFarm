using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class PostDAO
    {
        private readonly IMongoCollection<Post> _postCollection;
        private readonly IMongoCollection<PostCategory> _postCategoryCollection;

        public PostDAO(IMongoDatabase database)
        {
            _postCollection = database.GetCollection<Post>("Post");
            _postCategoryCollection = database.GetCollection<PostCategory>("PostCategory");
        }

        /// <summary>
        /// Searches for posts that contain the given keyword in their content.
        /// This method performs a case-insensitive search for posts that are not deleted 
        /// and contain the specified keyword in their content.
        /// </summary>
        /// <param name="keyword">The keyword to search for in the post content.</param>
        /// <returns>A list of posts that match the search criteria.</returns>
        public async Task<List<Post>> SearchPostsByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // If the keyword is null or whitespace, return an empty list.
                return new List<Post>();
            }

            var filterBuilder = Builders<Post>.Filter;
            // Build a filter to find posts that are not deleted and contain the keyword in the content.
            var filter = filterBuilder.Eq(p => p.IsDeleted, false) &
                         filterBuilder.Regex(p => p.PostContent, new BsonRegularExpression(keyword, "i"));

            // Execute the query on the Post collection and return the results as a list.
            return await _postCollection
                .Find(filter)
                .ToListAsync();
        }

        /// <summary>
        /// Searches for posts that belong to specific categories based on the provided category IDs.
        /// The method allows searching using either an AND logic (posts must belong to all specified categories) 
        /// or an OR logic (posts must belong to at least one specified category).
        /// </summary>
        /// <param name="categoryIds">A list of category IDs to search for.</param>
        /// <param name="isAndLogic">A boolean value indicating whether to use AND logic (true) or OR logic (false) 
        /// for filtering posts based on category membership.</param>
        /// <returns>A list of posts that belong to the specified categories based on the logic used.</returns>
        public async Task<List<Post>> SearchPostsByCategoriesAsync(List<string> categoryIds, bool isAndLogic)
        {
            if (categoryIds == null || categoryIds.Count == 0)
                // If no category IDs are provided, return an empty list.
                return new List<Post>();

            // Validate the categoryIds list to ensure all IDs are valid ObjectId strings.
            foreach (var categoryId in categoryIds)
            {
                if (!ObjectId.TryParse(categoryId, out _))
                    return new List<Post>();
            }

            // Retrieve PostIds from PostCategory collection based on the provided category IDs.
            var postIdGroups = await _postCategoryCollection
                .Find(pc => categoryIds.Contains(pc.CategoryId))
                .Project(pc => new { pc.PostId, pc.CategoryId })
                .ToListAsync();

            if (postIdGroups.Count == 0)
                // If no posts match the provided categories, return an empty list.
                return new List<Post>();

            // Determine valid PostIds based on the AND or OR logic.
            var validPostIds = new List<string>();
            if (isAndLogic)
            {
                // AND logic: Only include posts that belong to all specified categories.
                var postIdCounts = postIdGroups
                    .GroupBy(pc => pc.PostId)
                    .Select(g => new { PostId = g.Key, CategoryCount = g.Count() })
                    .Where(g => g.CategoryCount == categoryIds.Count)
                    .Select(g => g.PostId)
                    .ToList();

                validPostIds = postIdCounts;
            }
            else
            {
                // OR logic: Include posts that belong to at least one of the specified categories.
                validPostIds = postIdGroups.Select(pc => pc.PostId).Distinct().ToList();
            }

            if (validPostIds.Count == 0)
                // If no posts match the category criteria, return an empty list.
                return new List<Post>();

            // Retrieve the posts from the Post collection that match the valid PostIds, are not deleted, and are public.
            return await _postCollection
                .Find(p => validPostIds.Contains(p.PostId) && p.IsDeleted == false && p.PostScope == "Public")
                .ToListAsync();
        }
    }
}
