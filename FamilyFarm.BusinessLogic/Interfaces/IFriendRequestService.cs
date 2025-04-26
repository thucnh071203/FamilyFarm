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
        Task<List<Friend>> GetAllSendFriendRequests(string senderId);
        Task<List<Friend>> GetAllReceiveFriendRequests(string receiverId);
    }
}
