using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ISharePostRepository
    {
        Task<SharePost?> GetById(string? sharePostId);
        Task<List<SharePost>?> GetByAccId(string? postId);
        Task<List<SharePost>?> GetByPost(string? postId);
        Task<SharePost?> CreateAsync(SharePost? sharePost);
        Task<SharePost?> UpdateAsync(SharePost? request);
        Task<bool> HardDeleteAsync(string? sharePostId);
        Task<bool> SoftDeleteAsync(string? sharePostId);
        Task<bool> RestoreAsync(string? sharePostId);
        Task<bool> DisableAsync(string? postId);
        Task<List<SharePost>?> GetDeletedByAccId(string? accId);
    }
}
