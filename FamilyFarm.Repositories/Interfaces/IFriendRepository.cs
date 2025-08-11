using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IFriendRepository
    {
        Task<string> CheckIsFriendAsync(string senderId, string receiverId);
        Task<List<Account>> GetListFriends(string userId, string roleId);
        Task<List<Account>> GetListFollower(string receiverId);
        Task<List<Account>> GetListFollowing(string senderId, string roleId);
        Task<List<Account>>  GetListSuggestionFriends(string userId, int number);
        Task<List<Account>> GetSuggestedExperts(string userId, int number);
        Task<bool> Unfriend(string senderId, string receiverId);
        Task<(List<Account> Farmers, List<Account> Experts)> GetAvailableFarmersAndExpertsAsync(string accId);
        Task<List<Account>> SearchUsers(string userId, string keyword, int number);
    }
}
