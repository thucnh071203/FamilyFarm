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
    public class PostImageRepository : IPostImageRepository
    {
        private readonly PostImageDAO _dao;

        public PostImageRepository(PostImageDAO dao)
        {
            _dao = dao;
        }

        public async Task<PostImage?> CreatePostImage(PostImage? request)
        {
            return await _dao.CreatePostImage(request);
        }

        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            return await _dao.DeleteAllByPostId(post_id);
        }

        public async Task<bool> DeleteImageById(string? image_id)
        {
            return await _dao.DeleteImageById(image_id);
        }

        public async Task<PostImage?> GetPostImageById(string? image_id)
        {
            return await _dao.GetById(image_id);
        }

        public async Task<List<PostImage>?> GetPostImageByPost(string? post_id)
        {
            return await _dao.GetAllImageOfPost(post_id);
        }

        public async Task<bool> InactiveImagesByPostId(string? post_id)
        {
            return await _dao.InactiveAllByPostId(post_id);
        }

        public async Task<bool> ActiveImagesByPostId(string? post_id)
        {
            return await _dao.ActiveAllByPostId(post_id);
        }
        public async Task<List<string>> GetAllImage(string accId)
        {
            return await _dao.GetAllImage(accId);
        }
    }
}
