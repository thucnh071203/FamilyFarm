using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IChatService
    {
        Task<Chat> StartChatAsync(string user1Id, string user2Id);
        Task<ListChatResponseDTO> GetUserChatsAsync(string userId);
        Task<List<Chat>> SearchChatsByFullNameAsync(string userId, string fullName);
        Task<ListChatDetailsResponseDTO> GetChatMessagesAsync(string acc1Id, string acc2Id, int skip = 0, int take = 20);
        Task<SendMessageResponseDTO> SendMessageAsync(string senderId, SendMessageRequestDTO request);
        Task<bool> MarkMessagesAsSeenAsync(string chatId, string receiverId);
        Task<ChatDetail> RecallChatDetailByIdAsync(string chatDetailId);
        Task DeleteChatHistoryAsync(string chatDetailId);
        Task<string> GetChatResponseAsync(string userInput);
    }
}
