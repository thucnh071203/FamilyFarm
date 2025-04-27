using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IFriendRequestService
    {
        Task<FriendResponseDTO?> GetAllSendFriendRequests(string senderId);

        Task<FriendResponseDTO?> GetAllReceiveFriendRequests(string receveiId);
Task<bool> AcceptFriendRequestAsync(string friendId);
     Task<bool> RejectFriendRequestAsync(string friendId);
        Task<bool> SendFriendRequestAsync(string senderId, string receiverId);

    }
}
