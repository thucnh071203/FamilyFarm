using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Driver.Core.Servers;
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

        public async Task<List<Account>> GetSentFriendRequests(string senderId)
        {
            return await _requestDAO.GetSentFriendRequestsAsync(senderId);
        }
        public async Task<List<Account>> GetReceiveFriendRequests(string receveiId)
        {
            return await _requestDAO.GetReceiveFriendRequestsAsync(receveiId);
        }

        // Từ chối yêu cầu kết bạn
        public async Task<bool> AcceptFriendRequestAsync(string senderId, string receiverId)
        {
            // var friendRequest = await _requestDAO.GetFriendRequestAsync(friendId);

            if (senderId == null || receiverId == null)
            {
                return false;
            }

            return await _requestDAO.AcceptFriendRequestAsync(senderId, receiverId);
        }

        // Từ chối yêu cầu kết bạn
        public async Task<bool> RejectFriendRequestAsync(string senderId, string receiverId)
        {
            //var friendRequest = await _requestDAO.GetFriendRequestAsync(friendId);

            //if (friendRequest == null || friendRequest.Status != "Pending")
            //{
            //    return false;
            //}

            if (senderId == null || receiverId == null)
            {
                return false;
            }

            return await _requestDAO.RejectFriendRequestAsync(senderId, receiverId);
        }


        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
        {
            return await _requestDAO.CreateFriendRequestAsync(senderId, receiverId);
        }
    }
}
