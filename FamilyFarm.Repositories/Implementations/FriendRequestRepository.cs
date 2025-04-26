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
  
    public class FriendRequestRepository : IFriendRequestRepository
    {
        private readonly FriendRequestDAO _requestDAO;

        public FriendRequestRepository(FriendRequestDAO requestDAO)
        {
            _requestDAO = requestDAO;
        }

        public async Task<List<Friend>> GetSentFriendRequests(string senderId)
        {
            return await _requestDAO.GetSentFriendRequestsAsync(senderId);
        }
        public async Task<List<Friend>> GetReceiveFriendRequests(string receiverId)
        {
            return await _requestDAO.GetReceivedFriendRequestsAsync(receiverId);
        }

    
    }
}
