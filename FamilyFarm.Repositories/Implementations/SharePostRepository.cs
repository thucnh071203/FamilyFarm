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

        public async Task<SharePost?> GetById(string? sharePostId)
        {
            return await _sharePostDAO.GetById(sharePostId);
        }

        public SharePostRepository(SharePostDAO sharePostDAO)
        {
            _sharePostDAO = sharePostDAO;
        }

        public async Task<SharePost?> CreateAsync(SharePost? sharePost)
        {
            return await _sharePostDAO.CreateAsync(sharePost);
        }

        public async Task<SharePost?> UpdateAsyns(SharePost? request)
        {
            return await _sharePostDAO.UpdateAsyns(request);
        }

        public async Task<bool> HardDeleteAsyns(string? sharePostId)
        {
            return await _sharePostDAO.HardDeleteAsyns(sharePostId);
        }

        public async Task<bool> SoftDeleteAsyns(string? sharePostId)
        {
            return await _sharePostDAO.SoftDeleteAsyns(sharePostId);
        }
    }
}
