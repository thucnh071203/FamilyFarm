using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class ChatDetailRepository : IChatDetailRepository
    {
        private readonly ChatDetailDAO _chatDetailDAO;

        public ChatDetailRepository(ChatDetailDAO chatDetailDAO)
        {
            _chatDetailDAO = chatDetailDAO;
        }

        public async Task<ChatDetail> CreateChatDetailAsync(ChatDetail chatDetail)
        {
            return await _chatDetailDAO.CreateChatDetailAsync(chatDetail);
        }

        public async Task<List<ChatDetail>> GetChatDetailsByAccIdsAsync(string accId1, string accId2, int skip = 0, int take = 20)
        {
            return await _chatDetailDAO.GetChatDetailsByAccIdsAsync(accId1, accId2, skip, take);
        }

        public async Task MarkMessagesAsSeenAsync(string chatId, string receiverId)
        {
            await _chatDetailDAO.MarkMessagesAsSeenAsync(chatId, receiverId);
        }

        public async Task<ChatDetail> RecallChatDetailByIdAsync(string chatDetailId)
        {
            return await _chatDetailDAO.RecallChatDetailByIdAsync(chatDetailId);
        }

        public async Task DeleteChatDetailsByChatIdAsync(string chatId)
        {
            await _chatDetailDAO.DeleteChatDetailsByChatIdAsync(chatId);
        }

        public async Task<int> GetTotalChatDetailsCountAsync(string acc1Id, string acc2Id)
        {
            return await _chatDetailDAO.GetTotalChatDetailsCountAsync(acc1Id, acc2Id);
        }
    }
}
