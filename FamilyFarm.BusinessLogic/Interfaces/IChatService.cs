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
        Task<List<Chat>> GetUserChatsAsync(string userId);
        Task<List<Chat>> SearchChatsByFullNameAsync(string userId, string fullName);
        Task<List<ChatDetail>> GetChatMessagesAsync(string chatId);
        Task<SendMessageResponseDTO> SendMessageAsync(string senderId, SendMessageRequestDTO request);
        Task<ChatDetail> MarkAsSeenAsync(string chatDetailId);
        Task<ChatDetail> RevokeChatDetailByIdAsync(string chatDetailId);
        Task DeleteChatHistoryAsync(string chatDetailId);
    }
}
