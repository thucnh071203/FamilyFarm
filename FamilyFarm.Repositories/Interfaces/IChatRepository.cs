using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task CreateChatAsync(Chat chat);
        Task<Chat> GetChatByIdAsync(string chatId);
        Task<Chat> GetChatByUsersAsync(string user1Id, string user2Id);
        Task<List<Chat>> GetChatsByUserAsync(string userId);
        Task DeleteChatAsync(string chatId);
    }
}
