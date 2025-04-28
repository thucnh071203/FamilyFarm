using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> SearchPostsByKeyword(string keyword);
        Task<List<Post>> SearchPostsByCategories(List<string> categoryIds, bool isAndLogic);
        Task<Post?> CreatePost(Post? post);
        Task<Post?> UpdatePost(Post? post);
        Task<Post?> GetPostById(string? post_id);
    }
}
