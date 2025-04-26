using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository friendRepository;
        public FriendService(IFriendRepository friendRepository)
        {
            this.friendRepository = friendRepository;
        }

        public async Task<List<Account>> GetListFriends(string userid)
        {
            return await friendRepository.GetListFriends(userid);
        }
        public async Task<List<Account>> GetListFollower(string receiverId)
        {
            return await friendRepository.GetListFollower(receiverId);
        }
        public async Task<List<Account>> GetListFollowing(string senderId)
        {
            return await friendRepository.GetListFollowing(senderId);
        }
        public async Task<bool> Unfriend(string senderId, string receiverId)
        {
            return await friendRepository.Unfriend(senderId, receiverId);
        }

    }
}
