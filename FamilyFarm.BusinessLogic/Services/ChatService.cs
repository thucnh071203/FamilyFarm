using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository; 
        private readonly IChatDetailRepository _chatDetailRepository;  
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor to initialize the chat service with required repositories and SignalR context.
        /// </summary>
        /// <param name="chatRepository">The repository for managing chat data.</param>
        /// <param name="chatDetailRepository">The repository for managing chat messages (chat details).</param>
        /// <param name="chatHubContext">The SignalR hub context to send notifications to clients.</param>
        public ChatService(IChatRepository chatRepository, IChatDetailRepository chatDetailRepository, IHubContext<ChatHub> chatHubContext, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _chatDetailRepository = chatDetailRepository;
            _chatHubContext = chatHubContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Starts a new chat between two users, or retrieves the existing chat if it already exists.
        /// </summary>
        /// <param name="accId1">The ID of the first user.</param>
        /// <param name="accId2">The ID of the second user.</param>
        /// <returns>Returns the chat ID.</returns>
        public async Task<Chat> StartChatAsync(string accId1, string accId2)
        {
            // Get the existing chat between the two users.
            var existingChat = await _chatRepository.GetChatByUsersAsync(accId1, accId2);

            if (existingChat != null)
            {
                return existingChat;  // Return the existing chat if found.
            }

            // Create a new chat if it doesn't exist.
            var chat = new Chat
            {
                ChatId = ObjectId.GenerateNewId().ToString(),  // Generate a new unique chat ID.
                Acc1Id = accId1,
                Acc2Id = accId2,
                CreateAt = DateTime.UtcNow  // Set the creation timestamp.
            };

            // Save the new chat to the repository.
            await _chatRepository.CreateChatAsync(chat);
            return chat;  // Return the new chat.
        }

        /// <summary>
        /// Retrieves all chats associated with a specific user.
        /// </summary>
        /// <param name="accId">The user ID to retrieve chats for.</param>
        /// <returns>Returns a list of chats for the user.</returns>
        public async Task<List<Chat>> GetUserChatsAsync(string accId)
        {
            return await _chatRepository.GetChatsByUserAsync(accId);  // Fetch the user's chats from the repository.
        }

        /// <summary>
        /// Searches for chats with a specific user by full name.
        /// </summary>
        /// <param name="accId">The ID of the user searching for chats.</param>
        /// <param name="fullName">The full name to search for.</param>
        /// <returns>Returns a list of chats that match the search criteria.</returns>
        public async Task<List<Chat>> SearchChatsByFullNameAsync(string accId, string fullName)
        {
            return await _chatRepository.SearchChatsByFullNameAsync(accId, fullName);  // Search chats based on the user's full name.
        }

        /// <summary>
        /// Retrieves all messages in a specific chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve messages for.</param>
        /// <returns>Returns a list of chat details (messages) for the chat.</returns>
        public async Task<List<ChatDetail>> GetChatMessagesAsync(string chatId)
        {
            return await _chatDetailRepository.GetChatDetailsByChatIdAsync(chatId);  // Fetch the chat details (messages) for the provided chat ID.
        }

        /// <summary>
        /// Sends a message in a chat by mapping the request to a ChatDetail object, 
        /// notifying users via SignalR, and saving the message to the database.
        /// This method validates the input, assigns the sender ID, and returns a response indicating success or failure.
        /// </summary>
        /// <param name="senderId">The unique identifier of the sender, typically extracted from the authenticated user's token (e.g., account.AccId).</param>
        /// <param name="request">The DTO containing message details such as the message content, chat ID, and receiver ID.</param>
        /// <returns>
        /// A <see cref="SendMessageResponseDTO"/> object indicating the result of the operation. 
        /// If successful, it contains the saved message details in the <see cref="SendMessageResponseDTO.Data"/> property, 
        /// along with a success message and <c>Success</c> set to <c>true</c>. 
        /// If failed, it contains an error message and <c>Success</c> set to <c>false</c>.
        /// </returns>
        public async Task<SendMessageResponseDTO> SendMessageAsync(string senderId, SendMessageRequestDTO request)
        {
            // Validate the input: Check if the request is null or if required fields (ChatId, ReceiverId) are missing.
            if (request == null || string.IsNullOrEmpty(request.ChatId) || string.IsNullOrEmpty(request.ReceiverId))
            {
                // Return a failure response if validation fails.
                return new SendMessageResponseDTO
                {
                    Message = "Invalid message data: ChatId, and ReceiverId are required.",
                    Success = false
                };
            }

            // Map the SendMessageRequestDTO to a ChatDetail object using AutoMapper.
            // The mapping automatically assigns default values for ChatDetailId, SendAt (via MappingProfile),
            // and IsRevoked, IsSeen (via ChatDetail model defaults).
            var chatDetail = _mapper.Map<ChatDetail>(request);

            // Assign the SenderId to the ChatDetail object, using the senderId parameter provided by the controller.
            chatDetail.SenderId = senderId;

            // Notify both the sender and receiver via SignalR by sending the "ReceiveMessage" event.
            // This ensures real-time updates for both users involved in the chat.
            await _chatHubContext.Clients.Users(new[] { chatDetail.SenderId, chatDetail.ReceiverId })
                                 .SendAsync("ReceiveMessage", chatDetail);

            // Save the chat message to the database using the repository.
            var savedMessage = await _chatDetailRepository.CreateChatDetailAsync(chatDetail);

            // Check if the message was saved successfully.
            if (savedMessage == null)
            {
                // Return a failure response if the message could not be saved.
                // This typically happens if the ChatId, SenderId, or ReceiverId is not a valid ObjectId.
                return new SendMessageResponseDTO
                {
                    Message = "Failed to send the message.",
                    Success = false
                };
            }

            // Map the saved ChatDetail object to a SendMessageResponseDTO for the response.
            var response = _mapper.Map<SendMessageResponseDTO>(savedMessage);

            // Set the success message and status in the response.
            response.Message = "Message sent successfully.";
            response.Success = true;

            // Return the response containing the saved message details.
            return response;
        }

        /// <summary>
        /// Marks a message as "seen" by updating its "IsSeen" flag.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail (message) to mark as seen.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        public async Task<ChatDetail> MarkAsSeenAsync(string chatDetailId)
        {
            return await _chatDetailRepository.UpdateIsSeenAsync(chatDetailId);  // Update the message's "IsSeen" flag to true.
        }

        /// <summary>
        /// Deletes the entire chat history for a specific chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat for which to delete the history.</param>
        /// <returns>Returns a task representing the asynchronous delete operation.</returns>
        public async Task DeleteChatHistoryAsync(string chatId)
        {
            // Xóa tất cả các ChatDetail liên quan đến chatId
            await _chatDetailRepository.DeleteChatDetailsByChatIdAsync(chatId);

            // Xóa toàn bộ chat
            await _chatRepository.DeleteChatAsync(chatId);

            // Gửi thông báo đến người dùng rằng lịch sử chat đã bị xóa
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat != null)
            {
                await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                    .SendAsync("ChatHistoryDeleted", chatId);
            }
        }

        /// <summary>
        /// Revokes a specific chat detail (message) by setting its IsRevoked field to true.
        /// This method will also notify both users of the chat about the revocation of the message.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail (message) to revoke.</param>
        /// <returns>
        /// Returns the revoked ChatDetail object if the revocation is successful, or null if no message is found to revoke.
        /// </returns>
        public async Task<ChatDetail> RevokeChatDetailByIdAsync(string chatDetailId)
        {
            // Revoke a specific ChatDetail (marking it as revoked)
            var revokedChatDetail = await _chatDetailRepository.RevokeChatDetailByIdAsync(chatDetailId);
            if (revokedChatDetail == null)
                return null;  // If no message is found to revoke, return null.

            // Fetch the associated chat information to notify the users
            var chat = await _chatRepository.GetChatByIdAsync(revokedChatDetail.ChatId);
            if (chat != null)
            {
                // Notify both users that the chat history (message) has been revoked
                await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                    .SendAsync("ChatRevoked", revokedChatDetail.ChatId);
            }

            return revokedChatDetail;  // Return the revoked chat detail object.
        }
    }
}
