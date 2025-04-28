using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDAO _postDAO;

        public PostRepository(PostDAO postDAO)
        {
            _postDAO = postDAO;
        }

        public async Task<List<Post>> SearchPostsByKeyword(string keyword)
        {
            return await _postDAO.SearchPostsByKeywordAsync(keyword);
        }

        public async Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic)
        {
            return await _postDAO.SearchPostsByCategoriesAsync(categoryIds, isAndLogic);
        }

        public async Task<Post?> CreatePost(Post? post)
        {
            return await _postDAO.CreatePost(post);
        }

        public async Task<Post?> UpdatePost(Post? post)
        {
            return await _postDAO.UpdatePost(post);
        }

        public async Task<Post?> GetPostById(string? post_id)
        {
            return await _postDAO.GetById(post_id);
        }
    }
}
