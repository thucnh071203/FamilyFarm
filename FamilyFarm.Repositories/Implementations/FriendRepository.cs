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
        public async Task<List<Account>> GetListFriends(string userid)
        {
            return await _friendDao.GetListFriends(userid);
        }
        public async Task<List<Account>> GetListFollower(string receiverId)
        {
            return await _friendDao.GetListFollower(receiverId);
        }
        public async Task<List<Account>> GetListFollowing(string senderId)
        {
            return await _friendDao.GetListFollowing(senderId);
        }
        public async Task<bool> Unfriend(string senderId, string receiverId)
        {
            return await _friendDao.Unfriend(senderId, receiverId);
        }
    }
}
