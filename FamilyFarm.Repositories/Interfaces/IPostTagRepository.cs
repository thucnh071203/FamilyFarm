using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPostTagRepository
    {
        Task<PostTag?> CreatePostTag(PostTag? request);
        Task<List<PostTag>?> GetPostTagByPost(string? post_id);
        Task<bool> DeletePostTagById(string? post_tag_id);
        Task<bool> DeleteAllByPostId(string? post_id);
    }
}
