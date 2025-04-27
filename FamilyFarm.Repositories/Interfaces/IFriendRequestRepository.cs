using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IFriendRequestRepository
    {
        Task<List<Account>> GetSentFriendRequests(string senderId);

        Task<List<Account>> GetReceiveFriendRequests(string receveiId);
       Task<bool> AcceptFriendRequestAsync(string friendId);
        Task<bool> RejectFriendRequestAsync(string friendId);
        Task<bool> SendFriendRequestAsync(string senderId, string receiverId);
    }
}
