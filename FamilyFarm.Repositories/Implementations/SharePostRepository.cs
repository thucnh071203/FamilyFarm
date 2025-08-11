using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class SharePostRepository : ISharePostRepository
    {
        private readonly SharePostDAO _sharePostDAO;

        public SharePostRepository(SharePostDAO sharePostDAO)
        {
            _sharePostDAO = sharePostDAO;
        }

        public async Task<SharePost?> GetById(string? sharePostId)
        {
            return await _sharePostDAO.GetById(sharePostId);
        }

        public async Task<List<SharePost>?> GetByAccId(string? accId)
        {
            return await _sharePostDAO.GetByAccId(accId);
        }

        public async Task<SharePost?> CreateAsync(SharePost? sharePost)
        {
            return await _sharePostDAO.CreateAsync(sharePost);
        }

        public async Task<SharePost?> UpdateAsync(SharePost? request)
        {
            return await _sharePostDAO.UpdateAsync(request);
        }

        public async Task<bool> HardDeleteAsync(string? sharePostId)
        {
            return await _sharePostDAO.HardDeleteAsync(sharePostId);
        }

        public async Task<bool> SoftDeleteAsync(string? sharePostId)
        {
            return await _sharePostDAO.SoftDeleteAsync(sharePostId);
        }

        public async Task<List<SharePost>?> GetByPost(string? postId)
        {
            return await _sharePostDAO.GetByPost(postId);
        }

        public async Task<bool> RestoreAsync(string? sharePostId)
        {
            return await _sharePostDAO.RestoreAsync(sharePostId);
        }

        public async Task<bool> DisableAsync(string? sharePostId)
        {
            return await _sharePostDAO.DisableAsync(sharePostId);
        }

        public async Task<List<SharePost>?> GetDeletedByAccId(string? accId)
        {
            return await _sharePostDAO.GetDeletedByAccId(accId);
        }
    }
}
