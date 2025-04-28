using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class PostCategoryRepository : IPostCategoryRepository
    {
        private readonly PostCategoryDAO _dao;

        public PostCategoryRepository(PostCategoryDAO dao)
        {
            _dao = dao;
        }

        public async Task<PostCategory?> CreatePostCategory(PostCategory? request)
        {
            return await _dao.CreatePostCategory(request);
        }

        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            return await _dao.DeleteAllByPostId(post_id);
        }

        public async Task<bool> DeletePostCategoryById(string? post_category_id)
        {
            return await _dao.DeletePostCategoryById(post_category_id);
        }

        public async Task<List<PostCategory>?> GetCategoryByPost(string? post_id)
        {
            return await _dao.GetAllCategoryOfPost(post_id);
        }
    }
}
