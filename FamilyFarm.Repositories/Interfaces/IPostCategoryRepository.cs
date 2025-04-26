using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPostCategoryRepository
    {
        Task<PostCategory?> CreatePostCategory(PostCategory? request);
        Task<List<PostCategory>?> GetCategoryByPost(string? post_id);
    }
}
