using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IReactionPostService
    {
        Task<bool> ToggleReactionAsync(string postId, string accId, string categoryReactionId);
        Task<List<ReactionPost>> GetAllByPostAsync(string postId);
    }
}
