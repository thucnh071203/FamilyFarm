using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ISharePostTagRepository
    {
        Task<SharePostTag?> CreateAsyns(SharePostTag? request);
        Task<List<SharePostTag>?> GetAllBySharePost(string? sharePostId);
        Task<bool> DeleteTagById(string? sharePostTagId);
        Task<bool> DeleteAllBySharePostId(string? sharePostId);
    }
}
