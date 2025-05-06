using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDAO _chatDAO;

        public ChatRepository(ChatDAO chatDAO)
        {
            _chatDAO = chatDAO;
        }

        public async Task CreateChatAsync(Chat chat)
        {
            await _chatDAO.CreateChatAsync(chat);
        }

        public async Task DeleteChatAsync(string chatId)
        {
            await _chatDAO.DeleteChatAsync(chatId);
        }

        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _chatDAO.GetChatByIdAsync(chatId);
        }

        public async Task<Chat> GetChatByUsersAsync(string user1Id, string user2Id)
        {
            return await _chatDAO.GetChatByUsersAsync(user1Id, user2Id);
        }

        public async Task<List<Chat>> GetChatsByUserAsync(string userId)
        {
            return await _chatDAO.GetChatsByUserAsync(userId);
        }
    }
}
