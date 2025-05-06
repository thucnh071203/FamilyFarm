using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ChatDAO
    {
        private readonly IMongoCollection<Chat> _chats;

        /// <summary>
        /// Constructor to initialize the DAO with the MongoDB collections.
        /// </summary>
        /// <param name="database">MongoDB database instance.</param>
        public ChatDAO(IMongoDatabase database)
        {
            // Initialize the MongoDB collections for chats and accounts.
            _chats = database.GetCollection<Chat>("Chat");
        }

        /// <summary>
        /// Creates a new chat by inserting a new document in the "Chat" collection.
        /// </summary>
        /// <param name="chat">The chat object to be inserted.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        public async Task CreateChatAsync(Chat chat)
        {
            await _chats.InsertOneAsync(chat);  // Insert the new chat into the collection.
        }

        /// <summary>
        /// Retrieves a chat by its unique chat ID.
        /// </summary>
        /// <param name="chatId">The unique ID of the chat.</param>
        /// <returns>Returns the chat if found, or null if not found.</returns>
        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _chats.Find(c => c.ChatId == chatId)  // Search for chat by ID.
                .FirstOrDefaultAsync();  // Return the first matching chat, or null if not found.
        }

        /// <summary>
        /// Retrieves a chat between two users based on their user IDs.
        /// </summary>
        /// <param name="acc1Id">The user ID of the first user.</param>
        /// <param name="acc2Id">The user ID of the second user.</param>
        /// <returns>Returns the chat if found, or null if not found.</returns>
        public async Task<Chat> GetChatByUsersAsync(string acc1Id, string acc2Id)
        {
            // Search for a chat where either user1 is User1 and user2 is User2, or vice versa.
            return await _chats.Find(c =>
                (c.Acc1Id == acc1Id && c.Acc2Id == acc2Id) ||
                (c.Acc1Id == acc2Id && c.Acc2Id == acc1Id))
                .FirstOrDefaultAsync();  // Return the first matching chat, or null if not found.
        }

        /// <summary>
        /// Retrieves all chats associated with a given user.
        /// </summary>
        /// <param name="accId">The user ID of the user.</param>
        /// <returns>Returns a list of chats the user is part of.</returns>
        public async Task<List<Chat>> GetChatsByUserAsync(string accId)
        {
            if (!ObjectId.TryParse(accId, out _))
                return null;
            // Search for chats where the user is either User1 or User2.
            return await _chats.Find(c => c.Acc1Id == accId || c.Acc2Id == accId)
                .ToListAsync();  // Return the list of matching chats.
        }

        /// <summary>
        /// Deletes a chat by its chatId. This will remove all associated chat details/messages from the database.
        /// </summary>
        /// <param name="chatId">The ID of the chat to delete.</param>
        /// <returns>
        /// Returns nothing (void) after the deletion process. 
        /// If the provided chatId is not a valid ObjectId, the method will simply return without performing any action.
        /// </returns>
        public async Task DeleteChatAsync(string chatId)
        {
            if (!ObjectId.TryParse(chatId, out _))
                return;  // If chatId is not a valid ObjectId, return immediately without doing anything.

            await _chats.DeleteManyAsync(cd => cd.ChatId == chatId);  // Delete all chat details where the chatId matches.
        }
    }
}
