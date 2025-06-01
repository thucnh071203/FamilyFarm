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
       Task<bool> AcceptFriendRequestAsync(string senderId, string receiverId);
        Task<bool> RejectFriendRequestAsync(string senderId, string receiverId);
        Task<bool> SendFriendRequestAsync(string senderId, string receiverId);
    }
}
