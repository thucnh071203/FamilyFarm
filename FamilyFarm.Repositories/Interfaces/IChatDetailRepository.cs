using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IChatDetailRepository
    {
        Task<ChatDetail> CreateChatDetailAsync(ChatDetail chatDetail);
        Task<List<ChatDetail>> GetChatDetailsByAccIdsAsync(string accId1, string accId2, int skip = 0, int take = 20);
        Task MarkMessagesAsSeenAsync(string chatId, string receiverId);
        Task<ChatDetail> RecallChatDetailByIdAsync(string chatDetailId);
        Task DeleteChatDetailsByChatIdAsync(string chatId);
        Task<int> GetTotalChatDetailsCountAsync(string acc1Id, string acc2Id);
    }
}
