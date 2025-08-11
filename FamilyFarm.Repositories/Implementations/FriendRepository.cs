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
    public class FriendRepository : IFriendRepository
    {
        private readonly FriendDAO _friendDao;
        public FriendRepository(FriendDAO friendDao)
        {
            _friendDao = friendDao;
        }
        public async Task<List<Account>> GetListFriends(string userid, string roleId)
        {
            return await _friendDao.GetListFriends(userid, roleId);
        }
        public async Task<List<Account>> GetListFollower(string receiverId)
        {
            return await _friendDao.GetListFollower(receiverId);
        }
        public async Task<List<Account>> GetListFollowing(string senderId, string roleId)
        {
            return await _friendDao.GetListFollowing(senderId, roleId);
        }
        public async Task<bool> Unfriend(string senderId, string receiverId)
        {
            return await _friendDao.Unfriend(senderId, receiverId);
        }

        public async Task<List<Account>> GetListSuggestionFriends(string userId, int number)
        {
            return await _friendDao.GetListSuggestionFriends(userId, number);
        }
        public async Task<List<Account>> GetSuggestedExperts(string userId, int number)
        {
            return await _friendDao.GetSuggestedExperts(userId, number);
        }
        public async Task<(List<Account> Farmers, List<Account> Experts)> GetAvailableFarmersAndExpertsAsync(string accId)
        {
            return await _friendDao.GetAvailableFarmersAndExpertsAsync(accId);
        }

        public async Task<string> CheckIsFriendAsync(string senderId, string receiverId)
        {
            return await _friendDao.CheckIsFriendAsync(senderId, receiverId);
        }

        public async Task<List<Account>> SearchUsers(string userId, string keyword, int number)
        {
            return await _friendDao.SearchUsers(userId, keyword, number);
        }
    }
}
