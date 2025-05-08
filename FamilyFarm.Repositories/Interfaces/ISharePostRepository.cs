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
        Task<SharePost?> CreateAsync(SharePost? sharePost);
        Task<SharePost?> UpdateAsyns(SharePost? request);
        Task<bool> HardDeleteAsyns(string? sharePostId);
        Task<bool> SoftDeleteAsyns(string? sharePostId);
    }
}
