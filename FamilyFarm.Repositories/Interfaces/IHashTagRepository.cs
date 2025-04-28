using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IHashTagRepository
    {
        Task<HashTag?> CreateHashTag(HashTag? request);
        Task<List<HashTag>?> GetHashTagByPost(string? post_id);
        Task<bool> DeleteHashTagById(string? hashtag_id);
        Task<bool> DeleteAllByPostId(string? post_id);
    }
}
