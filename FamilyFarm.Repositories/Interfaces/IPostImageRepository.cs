using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPostImageRepository
    {
        Task<PostImage?> CreatePostImage(PostImage? request);
        Task<List<PostImage>?> GetPostImageByPost(string? post_id);
    }
}
