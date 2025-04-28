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
    public class PostTagRepository : IPostTagRepository
    {
        private readonly PostTagDAO _dao;

        public PostTagRepository(PostTagDAO dao)
        {
            _dao = dao;
        }

        public async Task<PostTag?> CreatePostTag(PostTag? request)
        {
            return await _dao.CreatePostTag(request);
        }

        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            return await _dao.DeleteAllByPostId(post_id);
        }

        public async Task<bool> DeletePostTagById(string? post_tag_id)
        {
            return await _dao.DeleteTagById(post_tag_id);
        }

        public async Task<List<PostTag>?> GetPostTagByPost(string? post_id)
        {
            return await _dao.GetAllPostTagOfPost(post_id);
        }
    }
}
