using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IReactionPostRepository
    {
        Task<ReactionPost> GetByPostAccAndReactionAsync(string postId, string accId, string categoryReactionId);
        Task<ReactionPost> GetByPostAndAccAsync(string postId, string accId);
        Task<List<ReactionPost>> GetAllByPostAsync(string postId);
        Task<ReactionPost> CreateAsync(ReactionPost reactionPost);
        Task<bool> UpdateAsync(string reactPostId, string categoryReactionId, bool isDeleted);
        Task<bool> DeleteAsync(string reactPostId);
        Task<bool> RestoreAsync(string reactPostId);
    }
}
