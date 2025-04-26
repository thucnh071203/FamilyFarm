using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRequestRepository _requestRepository;

        public FriendRequestService(IFriendRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<List<Friend>> GetAllSendFriendRequests(string senderId)
        {
            var sentRequests = await _requestRepository.GetSentFriendRequests(senderId);
            return sentRequests;
        }
        public async Task<List<Friend>> GetAllReceiveFriendRequests(string receveiId)
        {
            var receiveRequests = await _requestRepository.GetSentFriendRequests(receveiId);
            return receiveRequests;
        }


    }
}
