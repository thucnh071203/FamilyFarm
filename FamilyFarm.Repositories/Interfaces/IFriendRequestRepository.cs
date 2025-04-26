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
        Task<List<Friend>> GetSentFriendRequests(string senderId);
                Task<List<Friend>> GetReceiveFriendRequests(string receiverId);
    }
}
