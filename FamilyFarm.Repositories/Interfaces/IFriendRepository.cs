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
        Task<List<Account>> GetListFriends(string userId);
        Task<List<Account>> GetListFollower(string receiverId);
        Task<List<Account>> GetListFollowing(string senderId);
        Task<bool> Unfriend(string senderId, string receiverId);
    }
}
