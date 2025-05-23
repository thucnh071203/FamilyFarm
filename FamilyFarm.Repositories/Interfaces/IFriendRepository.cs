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
        Task<List<Account>> GetListFriends(string userId, string roleId);
        Task<List<Account>> GetListFollower(string receiverId);
        Task<List<Account>> GetListFollowing(string senderId, string roleId);
        Task<bool> Unfriend(string senderId, string receiverId);
    }
}
