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

        public async Task<List<ChatDetail>> GetChatDetailsByChatIdAsync(string chatId)
        {
            return await _chatDetailDAO.GetChatDetailsByChatIdAsync(chatId);
        }

        public async Task<ChatDetail> UpdateIsSeenAsync(string chatDetailId)
        {
            return await _chatDetailDAO.UpdateIsSeenAsync(chatDetailId);
        }

        public async Task<ChatDetail> RevokeChatDetailByIdAsync(string chatDetailId)
        {
            return await _chatDetailDAO.RevokeChatDetailByIdAsync(chatDetailId);
        }

        public async Task DeleteChatDetailsByChatIdAsync(string chatId)
        {
            await _chatDetailDAO.DeleteChatDetailsByChatIdAsync(chatId);
        }
    }
}
