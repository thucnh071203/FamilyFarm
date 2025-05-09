using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
        private readonly IUploadFileService _uploadFileService;
        private readonly INotificationService _notificationService;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryNotificationRepository _categoryNotificationRepository;

        /// <summary>
        /// Constructor to initialize the chat service with required repositories and SignalR context.
        /// </summary>
        /// <param name="chatRepository">The repository for managing chat data.</param>
        /// <param name="chatDetailRepository">The repository for managing chat messages (chat details).</param>
        /// <param name="chatHubContext">The SignalR hub context to send notifications to clients.</param>
        public ChatService(IChatRepository chatRepository, IChatDetailRepository chatDetailRepository, IHubContext<ChatHub> chatHubContext, IMapper mapper, IUploadFileService uploadFileService, INotificationService notificationService, IAccountRepository accountRepository, ICategoryNotificationRepository categoryNotificationRepository)
        {
            _chatRepository = chatRepository;
            _chatDetailRepository = chatDetailRepository;
            _chatHubContext = chatHubContext;
            _mapper = mapper;
            _uploadFileService = uploadFileService;
            _notificationService = notificationService;
            _accountRepository = accountRepository;
            _categoryNotificationRepository = categoryNotificationRepository;
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
            // 1. Lấy danh sách accountIds dựa trên fullName
            var accountIds = await _accountRepository.GetAccountIdsByFullNameAsync(fullName);

            // Nếu không tìm thấy tài khoản nào hoặc fullName rỗng, trả về danh sách rỗng
            if (!accountIds.Any())
                return new List<Chat>();

            // 2. Lấy tất cả các cuộc trò chuyện của người dùng
            var chats = await _chatRepository.GetChatsByUserAsync(accId);
            if (chats == null || !chats.Any())
                return new List<Chat>();

            // 3. Lọc các cuộc trò chuyện mà người dùng kia có ID trong accountIds
            var filteredChats = chats.Where(c =>
                (c.Acc1Id == accId && accountIds.Contains(c.Acc2Id)) ||
                (c.Acc2Id == accId && accountIds.Contains(c.Acc1Id))
            ).ToList();

            return filteredChats;
        }

        /// <summary>
        /// Retrieves all messages in a specific chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve messages for.</param>
        /// <returns>Returns a list of chat details (messages) for the chat.</returns>
        public async Task<List<ChatDetail>> GetChatMessagesAsync(string acc1Id, string acc2Id)
        {
            return await _chatDetailRepository.GetChatDetailsByAccIdsAsync(acc1Id, acc2Id);  // Fetch the chat details (messages) for the provided chat ID.
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
            const int TIME_THRESHOLD_MS = 5 * 60 * 1000; // 5 minutes

            if (request == null || string.IsNullOrEmpty(request.ReceiverId))
            {
                return new SendMessageResponseDTO { Message = "ReceiverId is required.", Success = false };
            }

            // Ensure chat exists (create if not)
            var chat = await _chatRepository.GetChatByUsersAsync(senderId, request.ReceiverId);
            if (chat == null)
            {
                chat = new Chat
                {
                    ChatId = ObjectId.GenerateNewId().ToString(),
                    Acc1Id = senderId,
                    Acc2Id = request.ReceiverId
                };
                await _chatRepository.CreateChatAsync(chat);
            }

            // Handle file upload if exists
            if (request.File?.Length > 0)
            {
                try
                {
                    var upload = request.File.ContentType.StartsWith("image/")
                        ? await _uploadFileService.UploadImage(request.File)
                        : await _uploadFileService.UploadOtherFile(request.File);

                    request.FileUrl = upload.UrlFile;
                    request.FileType = upload.TypeFile;
                    request.FileName = request.File.FileName;
                }
                catch (Exception ex)
                {
                    return new SendMessageResponseDTO { Message = $"File upload failed: {ex.Message}", Success = false };
                }
            }

            // Map and assign required fields
            var chatDetail = _mapper.Map<ChatDetail>(request);
            chatDetail.ChatId = chat.ChatId;
            chatDetail.SenderId = senderId;

            // Notify via SignalR
            await _chatHubContext.Clients.Users(new[] { senderId, request.ReceiverId })
                                 .SendAsync("ReceiveMessage", chatDetail);

            // Check for notification spam
            var messages = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(senderId, request.ReceiverId);
            var lastMessage = messages.OrderByDescending(c => c.SendAt).FirstOrDefault();
            bool shouldNotify = lastMessage == null ||
                                (chatDetail.SendAt - lastMessage.SendAt).TotalMilliseconds >= TIME_THRESHOLD_MS;

            var account = await _accountRepository.GetAccountByIdAsync(senderId);
            if (shouldNotify)
            {
                var notiRequest = new SendNotificationRequestDTO
                {
                    ReceiverIds = new List<string> { request.ReceiverId },
                    SenderId = senderId,
                    CategoryNotiId = "",
                    TargetId = chat.ChatId,
                    TargetType = "Chat",
                    Content = ""
                };

                var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);
                if (!notiResponse.Success)
                {
                    Console.WriteLine($"Notification failed: {notiResponse.Message}");
                }
            }

            // Save message
            var saved = await _chatDetailRepository.CreateChatDetailAsync(chatDetail);
            if (saved == null)
                return new SendMessageResponseDTO { Message = "Message send failed.", Success = false };

            var response = _mapper.Map<SendMessageResponseDTO>(saved);
            response.Message = "Message sent successfully.";
            response.Success = true;
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
        public async Task<ChatDetail> RecallChatDetailByIdAsync(string chatDetailId)
        {
            // Revoke a specific ChatDetail (marking it as revoked)
            var recalledChatDetail = await _chatDetailRepository.RecallChatDetailByIdAsync(chatDetailId);
            if (recalledChatDetail == null)
                return null;  // If no message is found to revoke, return null.

            // Fetch the associated chat information to notify the users
            var chat = await _chatRepository.GetChatByIdAsync(recalledChatDetail.ChatId);
            if (chat != null)
            {
                // Notify both users that the chat history (message) has been revoked
                await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                    .SendAsync("ChatRevoked", recalledChatDetail.ChatId);
            }

            return recalledChatDetail;  // Return the revoked chat detail object.
        }
    }
}
