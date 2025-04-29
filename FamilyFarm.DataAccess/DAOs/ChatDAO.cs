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
        private readonly IMongoCollection<Account> _accounts;

        /// <summary>
        /// Constructor to initialize the DAO with the MongoDB collections.
        /// </summary>
        /// <param name="database">MongoDB database instance.</param>
        public ChatDAO(IMongoDatabase database)
        {
            // Initialize the MongoDB collections for chats and accounts.
            _chats = database.GetCollection<Chat>("Chat");
            _accounts = database.GetCollection<Account>("Account");
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
        /// <param name="user1Id">The user ID of the first user.</param>
        /// <param name="user2Id">The user ID of the second user.</param>
        /// <returns>Returns the chat if found, or null if not found.</returns>
        public async Task<Chat> GetChatByUsersAsync(string user1Id, string user2Id)
        {
            // Search for a chat where either user1 is User1 and user2 is User2, or vice versa.
            return await _chats.Find(c =>
                (c.User1Id == user1Id && c.User2Id == user2Id) ||
                (c.User1Id == user2Id && c.User2Id == user1Id))
                .FirstOrDefaultAsync();  // Return the first matching chat, or null if not found.
        }



        /// <summary>
        /// Retrieves all chats associated with a given user.
        /// </summary>
        /// <param name="userId">The user ID of the user.</param>
        /// <returns>Returns a list of chats the user is part of.</returns>
        public async Task<List<Chat>> GetChatsByUserAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
                return null;
            // Search for chats where the user is either User1 or User2.
            return await _chats.Find(c => c.User1Id == userId || c.User2Id == userId)
                .ToListAsync();  // Return the list of matching chats.
        }

        /// <summary>
        /// Searches for chats associated with a user by matching the full name of the other user in the chat.
        /// </summary>
        /// <param name="userId">The user ID of the searching user.</param>
        /// <param name="fullName">The full name of the user to search for in the chats.</param>
        /// <returns>Returns a list of chats where the other user has a full name matching the search term.</returns>
        public async Task<List<Chat>> SearchChatsByFullNameAsync(string userId, string fullName)
        {
            if (!ObjectId.TryParse(userId, out _))
                return null;

            // Use a case-insensitive regex to search for accounts with a full name matching the search term.
            var filter = Builders<Account>.Filter.Regex(
                a => a.FullName,
                new MongoDB.Bson.BsonRegularExpression(fullName, "i")  // Case-insensitive search.
            );

            // Find accounts matching the search criteria.
            var accounts = await _accounts.Find(filter).ToListAsync();
            var accountIds = accounts.Select(a => a.AccId).ToList();  // Extract the account IDs.

            // Search for chats where the user is part of the chat and the other participant is in the matching accounts.
            return await _chats.Find(c =>
                (c.User1Id == userId && accountIds.Contains(c.User2Id)) ||
                (c.User2Id == userId && accountIds.Contains(c.User1Id)))
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
