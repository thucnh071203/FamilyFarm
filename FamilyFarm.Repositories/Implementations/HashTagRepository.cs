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
    public class HashTagRepository : IHashTagRepository
    {
        private readonly HashtagDAO _dao;

        public HashTagRepository(HashtagDAO dao)
        {
            _dao = dao;
        }

        public async Task<HashTag?> CreateHashTag(HashTag? request)
        {
            return await _dao.CreateHashTag(request);
        }

        public async Task<bool> DeleteAllByPostId(string? post_id)
        {
            return await _dao.DeleteAllByPostId(post_id);
        }

        public async Task<bool> DeleteHashTagById(string? hashtag_id)
        {
            return await _dao.DeleteHashTagById(hashtag_id);
        }

        public async Task<List<HashTag>?> GetHashTagByPost(string? post_id)
        {
            return await _dao.GetAllHashTagOfPost(post_id);
        }
    }
}
