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
    public class SharePostTagRepository : ISharePostTagRepository
    {
        private readonly SharePostTagDAO _sharePostTagDAO;

        public SharePostTagRepository(SharePostTagDAO sharePostTagDAO)
        {
            _sharePostTagDAO = sharePostTagDAO;
        }

        public async Task<SharePostTag?> CreateAsyns(SharePostTag? request)
        {
            return await _sharePostTagDAO.CreateAsyns(request);
        }

        public async Task<bool> DeleteAllBySharePostId(string? sharePostId)
        {
            return await _sharePostTagDAO.DeleteAllBySharePostId(sharePostId);
        }

        public async Task<bool> DeleteTagById(string? sharePostTagId)
        {
            return await _sharePostTagDAO.DeleteTagById(sharePostTagId);
        }

        public async Task<List<SharePostTag>?> GetAllBySharePost(string? sharePostId)
        {
            return await _sharePostTagDAO.GetAllBySharePost(sharePostId);
        }
    }
}
