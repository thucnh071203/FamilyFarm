using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IFriendService
    {
        Task<string> CheckIsFriendAsync(string senderId, string receiverId);
        Task<FriendResponseDTO?> GetListFriends(string userId);
        Task<FriendResponseDTO?> GetListFollower(string receiverId);
        Task<FriendResponseDTO?> GetListFollowing(string senderId);
        Task<bool> Unfriend(string senderId, string receiverId);
        Task<FriendResponseDTO> MutualFriend(string userId, string otherId);
        Task<FriendResponseDTO?> GetListSuggestionFriends(string userId, int number);
        Task<FriendResponseDTO?> GetSuggestedExperts(string userId, int number);
        Task<List<FriendResponseDTO>> GetAvailableFarmersAndExpertsAsync(string accId);
        Task<FriendResponseDTO?> SearchUsers(string userId, string keyword, int number);
    }
}
