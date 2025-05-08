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

        public async Task<SharePost?> CreateAsync(SharePost? sharePost)
        {
            return await _sharePostDAO.CreateAsync(sharePost);
        }
    }
}
