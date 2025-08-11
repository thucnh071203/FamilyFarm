using AutoMapper;
using FamilyFarm.BusinessLogic.Config;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        private readonly INotificationTemplateService _notificationTemplate;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _instructions;
        private const string CohereApiKey = "oJW30Myk7DDQEglvMxyEZyI5gSTCyb7DcNTRVDLt";
        private const string CohereEndpoint = "https://api.cohere.ai/v1/generate";
        private readonly string _initializationError;


        /// <summary>
        /// Constructor to initialize the chat service with required repositories and SignalR context.
        /// </summary>
        /// <param name="chatRepository">The repository for managing chat data.</param>
        /// <param name="chatDetailRepository">The repository for managing chat messages (chat details).</param>
        /// <param name="chatHubContext">The SignalR hub context to send notifications to clients.</param>
        public ChatService(IChatRepository chatRepository, IChatDetailRepository chatDetailRepository, IHubContext<ChatHub> chatHubContext, IMapper mapper, IUploadFileService uploadFileService, INotificationService notificationService, IAccountRepository accountRepository, ICategoryNotificationRepository categoryNotificationRepository, INotificationTemplateService notificationTemplate, IHttpClientFactory httpClientFactory)
        {
            _chatRepository = chatRepository;
            _chatDetailRepository = chatDetailRepository;
            _chatHubContext = chatHubContext;
            _mapper = mapper;
            _uploadFileService = uploadFileService;
            _notificationService = notificationService;
            _accountRepository = accountRepository;
            _categoryNotificationRepository = categoryNotificationRepository;
            _notificationTemplate = notificationTemplate;
            _httpClientFactory = httpClientFactory;

            try
            {
                string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\FamilyFarm.BusinessLogic\\Data"));
                string filePath = Path.Combine(basePath, "Instruction.docx");

                if (!File.Exists(filePath))
                {
                    _initializationError = "Instruction file not found. Please contact admin.";
                }
                else
                {
                    _instructions = InstructionReader.ReadInstructions(filePath);
                }

                if (string.IsNullOrEmpty(CohereApiKey))
                {
                    _initializationError = "AI service is not properly configured (missing API key).";
                }
            }
            catch (Exception ex)
            {
                _initializationError = "Unexpected error during chat service initialization.";
            }
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
        //public async Task<List<Chat>> GetUserChatsAsync(string accId)
        //{
        //    return await _chatRepository.GetChatsByUserAsync(accId);  // Fetch the user's chats from the repository.
        //}
        public async Task<ListChatResponseDTO> GetUserChatsAsync(string accId)
        {
            var response = new ListChatResponseDTO();

            // Lấy danh sách cuộc trò chuyện
            var chats = await _chatRepository.GetChatsByUserAsync(accId);
            if (chats == null)
            {
                response.Success = false;
                response.Message = "No chats found for the user.";
                return response;
            }

            // Chuẩn bị danh sách ChatDTO
            var listChatDTOs = new List<ChatDTO>();
            foreach (var chat in chats)
            {
                // Xác định ID của đối phương
                var otherUserId = chat.Acc1Id == accId ? chat.Acc2Id : chat.Acc1Id;
                // Lấy thông tin tài khoản của đối phương
                var receiver = await _accountRepository.GetAccountById(otherUserId);
                if (receiver == null)
                {
                    // Xử lý trường hợp không tìm thấy tài khoản đối phương
                    continue; // Bỏ qua cuộc trò chuyện này hoặc xử lý theo cách khác
                }

                var receiverDTO = _mapper.Map<MyProfileDTO>(receiver);

                var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(chat.Acc1Id, chat.Acc2Id);

                // Map ChatDTO từ Chat
                var chatDTO = _mapper.Map<ChatDTO>(chat);
                chatDTO.Receiver = receiverDTO;
                // Lấy tin nhắn cuối cùng
                var lastMessage = chatDetails.OrderByDescending(c => c.SendAt).FirstOrDefault(); // Lấy tin nhắn mới nhất
                if (lastMessage != null)
                {
                    chatDTO.LastMessageAccId = lastMessage.SenderId ?? "";
                    chatDTO.LastMessage = lastMessage.Message ?? (lastMessage.FileName != null ? "File: " + lastMessage.FileName : "No messages yet");
                    chatDTO.LastMessageAt = lastMessage.SendAt;
                }
                else
                {
                    chatDTO.LastMessageAccId = "";
                    chatDTO.LastMessage = "No messages yet";
                    chatDTO.LastMessageAt = null;
                }

                // Lấy số tin nhắn chưa đọc
                var unreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == accId);
                chatDTO.UnreadCount = unreadCount;

                listChatDTOs.Add(chatDTO);
            }

            // Sắp xếp theo tin chat có tin nhắn mới nhất
            response.Chats = listChatDTOs
                .OrderByDescending(c => c.UnreadCount > 0)  // Ưu tiên có tin nhắn chưa đọc
                .ThenByDescending(c => c.LastMessageAt)     // Sau đó theo thời gian tin nhắn cuối
                .ToList(); response.unreadChatCount = listChatDTOs.Count(c => c.UnreadCount != 0);
            response.Message = "Chats retrieved successfully.";
            return response;
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
        public async Task<ListChatDetailsResponseDTO> GetChatMessagesAsync(string acc1Id, string acc2Id, int skip = 0, int take = 20)
        {
            var response = new ListChatDetailsResponseDTO();

            // Lấy tổng số tin nhắn không bị recalled
            var totalMessages = await _chatDetailRepository.GetTotalChatDetailsCountAsync(acc1Id, acc2Id);

            // Lấy danh sách tin nhắn với phân trang
            var messages = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(acc1Id, acc2Id, skip, take);
            if (messages == null || !messages.Any())
            {
                response.Success = messages != null;
                response.Message = "No messages found for this chat.";
                response.TotalMessages = totalMessages;
                return response;
            }

            response.ChatDetails = messages;
            response.TotalMessages = totalMessages;
            response.Message = "Messages retrieved successfully.";
            return response;
        }

        /// <summary>
        /// Marks all unseen messages in the specified chat as seen by the given receiver,
        /// and broadcasts a "MessageSeen" event to all connected clients.
        /// </summary>
        /// <param name="chatId">The ID of the chat whose messages are to be marked as seen.</param>
        /// <param name="receiverId">The ID of the receiver who has seen the messages.</param>
        /// <returns>
        /// True if the operation was initiated successfully; otherwise, false if the chatId or receiverId is null or empty.
        /// </returns>
        public async Task<bool> MarkMessagesAsSeenAsync(string chatId, string receiverId)
        {
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(receiverId))
                return false;

            await _chatDetailRepository.MarkMessagesAsSeenAsync(chatId, receiverId);

            // Lấy thông tin chat để tạo chatDTO
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null) return false;

            var otherUserId = chat.Acc1Id == receiverId ? chat.Acc2Id : chat.Acc1Id;
            var receiver = await _accountRepository.GetAccountById(otherUserId);
            if (receiver == null) return false;

            var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(chat.Acc1Id, chat.Acc2Id);
            var unreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == receiverId);

            var chatDTO = _mapper.Map<ChatDTO>(chat);
            chatDTO.Receiver = _mapper.Map<MyProfileDTO>(receiver);
            var lastMessage = chatDetails.OrderByDescending(c => c.SendAt).FirstOrDefault();
            if (lastMessage != null)
            {
                chatDTO.LastMessageAccId = lastMessage.SenderId ?? "";
                chatDTO.LastMessage = lastMessage.Message ?? (lastMessage.FileName != null ? "File: " + lastMessage.FileName : "No messages yet");
                chatDTO.LastMessageAt = lastMessage.SendAt;
            }
            chatDTO.UnreadCount = unreadCount;

            // Gửi thông báo tới cả hai người dùng
            await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                                 .SendAsync("MessageSeen", chatId, chatDTO);

            return true;
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
            const int TIME_THRESHOLD_MS = 5 * 60 * 1000;

            if (request == null || string.IsNullOrEmpty(request.ReceiverId))
            {
                return new SendMessageResponseDTO { Message = "ReceiverId is required.", Success = false };
            }

            // Ensure chat exists
            var chat = await _chatRepository.GetChatByUsersAsync(senderId, request.ReceiverId);
            if (chat == null)
            {
                chat = new Chat
                {
                    Acc1Id = senderId,
                    Acc2Id = request.ReceiverId
                };
                await _chatRepository.CreateChatAsync(chat);
            }

            // Handle file upload
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

            // Map and assign fields
            var chatDetail = _mapper.Map<ChatDetail>(request);
            chatDetail.ChatId = chat.ChatId;
            chatDetail.SenderId = senderId;
            chatDetail.SendAt = DateTime.UtcNow;

            // Save message
            var saved = await _chatDetailRepository.CreateChatDetailAsync(chatDetail);
            if (saved == null)
                return new SendMessageResponseDTO { Message = "Message send failed.", Success = false };

            // Construct chatDTO
            var receiver = await _accountRepository.GetAccountById(request.ReceiverId);
            if (receiver == null)
            {
                Console.WriteLine($"Receiver not found for receiverId: {request.ReceiverId}");
                return new SendMessageResponseDTO { Message = "Receiver not found.", Success = false };
            }

            var senderChatDTO = new ChatDTO
            {
                ChatId = chat.ChatId,
                Receiver = _mapper.Map<MyProfileDTO>(receiver),
                LastMessageAccId = chatDetail.SenderId ?? "",
                LastMessage = chatDetail.Message ?? (chatDetail.FileName != null ? "File: " + chatDetail.FileName : "No messages yet"),
                LastMessageAt = chatDetail.SendAt,
                UnreadCount = 0 // Người gửi không có tin nhắn chưa đọc
            };

            // Construct chatDTO for receiver
            var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(senderId, request.ReceiverId);
            var unreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == request.ReceiverId);
            var receiverChatDTO = new ChatDTO
            {
                ChatId = chat.ChatId,
                Receiver = _mapper.Map<MyProfileDTO>(await _accountRepository.GetAccountById(senderId)), // Người nhận thấy thông tin người gửi
                LastMessageAccId = chatDetail.SenderId ?? "",
                LastMessage = chatDetail.Message ?? (chatDetail.FileName != null ? "File: " + chatDetail.FileName : "No messages yet"),
                LastMessageAt = chatDetail.SendAt,
                UnreadCount = unreadCount // Số tin nhắn chưa đọc cho người nhận
            };

            // Notify via SignalR
            await _chatHubContext.Clients.User(senderId).SendAsync("ReceiveMessage", chatDetail, senderChatDTO);
            await _chatHubContext.Clients.User(request.ReceiverId).SendAsync("ReceiveMessage", chatDetail, receiverChatDTO);

            // Check for notification spam
            var messages = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(senderId, request.ReceiverId);
            var lastMessage = messages.OrderByDescending(c => c.SendAt).Skip(1).FirstOrDefault();
            bool shouldNotify = lastMessage == null ||
                                (chatDetail.SendAt - lastMessage.SendAt).TotalMilliseconds >= TIME_THRESHOLD_MS;

            //var categoryNotification = await _categoryNotificationRepository.GetByNameAsync("Chat");
            //if (categoryNotification == null)
            //{
            //    throw new InvalidOperationException("Chat notification category not found.");
            //}

            var account = await _accountRepository.GetAccountByIdAsync(senderId);
            if (shouldNotify)
            {
                var notiRequest = new SendNotificationRequestDTO
                {
                    ReceiverIds = new List<string> { request.ReceiverId },
                    SenderId = senderId,
                    CategoryNotiId = "681370cd5908b0f4fb0cd0f8",
                    TargetId = chat.ChatId,
                    TargetType = "Chat", //để link tới notifi gốc Post, Chat, Process, ...
                    Content = "You have a new message from " + account?.FullName
                };

                var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
                if (!notiResponse.Success)
                {
                    Console.WriteLine($"Notification failed: {notiResponse.Message}");
                }
            }

            var response = _mapper.Map<SendMessageResponseDTO>(saved);
            response.Message = "Message sent successfully.";
            response.Success = true;
            return response;
        }

        /// <summary>
        /// Deletes the entire chat history for a specific chat.
        /// </summary>
        /// <param name="chatId">The ID of the chat for which to delete the history.</param>
        /// <returns>Returns a task representing the asynchronous delete operation.</returns>
        public async Task DeleteChatHistoryAsync(string chatId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null) return;

            await _chatDetailRepository.DeleteChatDetailsByChatIdAsync(chatId);
            await _chatRepository.DeleteChatAsync(chatId);

            await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                                 .SendAsync("ChatHistoryDeleted", chatId);
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
            var recalledChatDetail = await _chatDetailRepository.RecallChatDetailByIdAsync(chatDetailId);
            if (recalledChatDetail == null)
                return null;

            var chat = await _chatRepository.GetChatByIdAsync(recalledChatDetail.ChatId);
            if (chat != null)
            {
                await _chatHubContext.Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id })
                                     .SendAsync("ChatRecalled", recalledChatDetail.ChatId, chatDetailId);
            }

            return recalledChatDetail;
        }

        public async Task<string> GetChatResponseAsync(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
                return "Please provide a question.";

            var prompt = $@"You are a helpful assistant guiding users on how to use a website.
                Instructions: {_instructions}
                User Input: {userInput}
                Please provide feedback based on the above instructions, using only the relevant part that matches the user's request in the order of steps. If the relevant part is not found, please politely notify the user.
                Response:";
            try
            {

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CohereApiKey);
                client.Timeout = TimeSpan.FromSeconds(30); // Set timeout

                var payload = new
                {
                    model = "command-r-plus", // Cohere's latest model
                    prompt = prompt,
                    max_tokens = 1000, // Adjust based on your needs
                    temperature = 0.7, // Balance between creativity and consistency
                    stop_sequences = new[] { "\n\nUser:", "Human:", "Assistant:" },
                    truncate = "END"
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(CohereEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    return $"Sorry, I encountered an error while processing your request. Status: {response.StatusCode}";
                }

                var jsonDoc = JsonDocument.Parse(responseContent);
                var result = jsonDoc.RootElement.GetProperty("generations")[0].GetProperty("text").GetString();

                var cleanResult = result?.Trim() ?? "No response generated.";

                return cleanResult;
            }
            catch (TaskCanceledException ex)
            {
                return "Request timed out. Please try again with a shorter message.";
            }
            catch (HttpRequestException ex)
            {
                return "Network error occurred. Please check your connection and try again.";
            }
            catch (JsonException ex)
            {
                return "Error parsing response. Please try again.";
            }
            catch (Exception ex)
            {
                return $"An unexpected error occurred: {ex.Message}";
            }
        }
    }
}
