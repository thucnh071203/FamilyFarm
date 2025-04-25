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
    public class ReactionPostRepository : IReactionPostRepository
    {
        private readonly ReactionPostDAO _reactionPostDAO;

        public ReactionPostRepository(ReactionPostDAO reactionPostDAO)
        {
            _reactionPostDAO = reactionPostDAO;
        }

        public async Task<ReactionPost> GetByPostAccAndReactionAsync(string postId, string accId, string categoryReactionId)
        {
            return await _reactionPostDAO.GetByPostAccAndReactionAsync(postId, accId, categoryReactionId);
        }

        public async Task<ReactionPost> GetByPostAndAccAsync(string postId, string accId)
        {
            return await _reactionPostDAO.GetByPostAndAccAsync(postId, accId);
        }

        public async Task<List<ReactionPost>> GetAllByPostAsync(string postId)
        {
            return await _reactionPostDAO.GetAllByPostAsync(postId);
        }

        public async Task<ReactionPost> CreateAsync(ReactionPost reactionPost)
        {
            return await _reactionPostDAO.CreateAsync(reactionPost);
        }

        public async Task<bool> UpdateAsync(string reactPostId, string categoryReactionId, bool isDeleted)
        {
            return await _reactionPostDAO.UpdateAsync(reactPostId, categoryReactionId, isDeleted);
        }

        public async Task<bool> DeleteAsync(string reactPostId)
        {
            return await _reactionPostDAO.DeleteAsync(reactPostId);
        }

        public async Task<bool> RestoreAsync(string reactPostId)
        {
            return await _reactionPostDAO.RestoreAsync(reactPostId);
        }
    }
}
