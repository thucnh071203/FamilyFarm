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
    public class ChatDetailDAO
    {
        private readonly IMongoCollection<ChatDetail> _chatDetails;

        /// <summary>
        /// Constructor to initialize the DAO with the MongoDB collection for chat details.
        /// </summary>
        /// <param name="database">MongoDB database instance.</param>
        public ChatDetailDAO(IMongoDatabase database)
        {
            // Initialize the MongoDB collection for chat details.
            _chatDetails = database.GetCollection<ChatDetail>("ChatDetail");
        }

        /// <summary>
        /// Creates a new chat detail by inserting a new document in the "ChatDetail" collection.
        /// </summary>
        /// <param name="chatDetail">The chat detail object to be inserted.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        public async Task<ChatDetail> CreateChatDetailAsync(ChatDetail chatDetail)
        {
            if (!ObjectId.TryParse(chatDetail.ChatId, out _)
                || !ObjectId.TryParse(chatDetail.SenderId, out _)
                || !ObjectId.TryParse(chatDetail.ReceiverId, out _))
            {
                return null;
            }
            chatDetail.ChatDetailId = ObjectId.GenerateNewId().ToString();
            await _chatDetails.InsertOneAsync(chatDetail);  // Insert the new chat detail into the collection.
            return chatDetail;  // Return the same object after insertion
        }

        /// <summary>
        /// Retrieves all chat details for a conversation between two users identified by their account IDs.
        /// </summary>
        /// <param name="senderId">The ID of the first user (sender).</param>
        /// <param name="receiverId">The ID of the second user (receiver).</param>
        /// <returns>Returns a sorted list of chat details for the conversation between the two users.</returns>
        public async Task<List<ChatDetail>> GetChatDetailsByAccIdsAsync(string senderId, string receiverId)
        {
            if (!ObjectId.TryParse(senderId, out _) || !ObjectId.TryParse(receiverId, out _))
                return null;

            var filter = Builders<ChatDetail>.Filter.Or(
                Builders<ChatDetail>.Filter.And(
                    Builders<ChatDetail>.Filter.Eq(cd => cd.SenderId, senderId),
                    Builders<ChatDetail>.Filter.Eq(cd => cd.ReceiverId, receiverId)
                ),
                Builders<ChatDetail>.Filter.And(
                    Builders<ChatDetail>.Filter.Eq(cd => cd.SenderId, receiverId),
                    Builders<ChatDetail>.Filter.Eq(cd => cd.ReceiverId, senderId)
                )
            );

            return await _chatDetails.Find(filter)
                .SortBy(cd => cd.SendAt)
                .ToListAsync();
        }


        /// <summary>
        /// Updates the "IsSeen" status of a specific chat detail to "true".
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail to update.</param>
        /// <returns>Returns a task representing the asynchronous update operation.</returns>
        public async Task<ChatDetail> UpdateIsSeenAsync(string chatDetailId)
        {
            if (!ObjectId.TryParse(chatDetailId, out _))
                return null;

            // Create a filter to find the specific chat detail by its ID.
            var filter = Builders<ChatDetail>.Filter.Eq(cd => cd.ChatDetailId, chatDetailId);

            // Create an update operation to set "IsSeen" to true.
            var update = Builders<ChatDetail>.Update.Set(cd => cd.IsSeen, true);

            // Perform the update operation.
            var result = await _chatDetails.UpdateOneAsync(filter, update);

            // Check if the update was successful (matched documents).
            if (result.MatchedCount == 0)
            {
                return null;  // No document was found with the provided ID.
            }

            // If update was successful, retrieve the updated chat detail from the database.
            return await _chatDetails.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes a specific chat detail by its ID and returns the deleted chat detail.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail to be deleted.</param>
        /// <returns>The deleted <see cref="ChatDetail"/> if found; otherwise, null.</returns>
        public async Task DeleteChatDetailsByChatIdAsync(string chatId)
        {
            if (!ObjectId.TryParse(chatId, out _))
                return;

            await _chatDetails.DeleteManyAsync(cd => cd.ChatId == chatId);
        }

        /// <summary>
        /// Marks a specific chat detail as revoked by setting IsRevoked to true.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail to be marked as revoked.</param>
        /// <returns>The updated <see cref="ChatDetail"/> if found; otherwise, null.</returns>
        public async Task<ChatDetail> RecallChatDetailByIdAsync(string chatDetailId)
        {
            if (!ObjectId.TryParse(chatDetailId, out _))
                return null;

            var filter = Builders<ChatDetail>.Filter.Eq(cd => cd.ChatDetailId, chatDetailId);

            // Find the chat detail before updating
            var chatDetail = await _chatDetails.Find(filter).FirstOrDefaultAsync();
            if (chatDetail == null)
                return null;

            // Update IsRevoked to true
            var update = Builders<ChatDetail>.Update.Set(cd => cd.IsRecalled, true);
            await _chatDetails.UpdateOneAsync(filter, update);

            // Update the in-memory object to reflect the change
            chatDetail.IsRecalled = true;

            return chatDetail;
        }
    }
}
