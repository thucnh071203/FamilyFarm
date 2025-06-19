using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class ChatHub : Hub
    {
        // A static dictionary to store the connection IDs for each account.
        // The key is the account ID, and the value is a list of connection IDs associated with that account.
        private static readonly Dictionary<string, List<string>> _accConnections = new();
        private readonly IChatRepository _chatRepository;
        private readonly IChatDetailRepository _chatDetailRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public ChatHub(
            IChatRepository chatRepository,
            IChatDetailRepository chatDetailRepository,
            IAccountRepository accountRepository,
            IMapper mapper)
        {
            _chatRepository = chatRepository;
            _chatDetailRepository = chatDetailRepository;
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// This method is invoked when a new connection is established.
        /// It retrieves the account ID from the query string and associates the connection ID with the account.
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // Get the account ID from the query string.
            var accId = Context.GetHttpContext()?.Request.Query["accId"].ToString();

            // If the account ID is not present, reject the connection and log a message.
            if (string.IsNullOrEmpty(accId))
            {
                return;
            }

            // Lock the connection dictionary to ensure thread safety while adding the connection ID.
            lock (_accConnections)
            {
                // If the account ID is not already in the dictionary, initialize it with an empty list.
                if (!_accConnections.ContainsKey(accId))
                {
                    _accConnections[accId] = new List<string>();
                }
                // Add the current connection ID to the list for the account.
                _accConnections[accId].Add(Context.ConnectionId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, accId);
            //Console.WriteLine($"[SignalR] Add connection {Context.ConnectionId} to group {accId}");
            // Call the base method to complete the connection process.
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// This method is invoked when a connection is disconnected.
        /// It removes the connection ID from the dictionary and logs the disconnection.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Find the account ID associated with the current connection ID.
            var accId = _accConnections.FirstOrDefault(x => x.Value.Contains(Context.ConnectionId)).Key;

            // If the connection ID is associated with an account, remove it from the list.
            if (!string.IsNullOrEmpty(accId))
            {
                lock (_accConnections)
                {
                    // Remove the current connection ID from the list for the account.
                    _accConnections[accId].Remove(Context.ConnectionId);

                    // If there are no remaining connections for the account, remove the account from the dictionary.
                    if (_accConnections[accId].Count == 0)
                    {
                        _accConnections.Remove(accId);
                    }
                }
            }

            // Call the base method to complete the disconnection process.
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// This method is called to notify users that a new message has been sent.
        /// It sends the "ReceiveMessage" event to both the sender and receiver.
        /// </summary>
        /// <param name="chatDetail">The chat message details to be sent to the clients.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <param name="receiverId">The ID of the receiver.</param>
        /// <returns></returns>
        public async Task SendMessage(ChatDetail chatDetail, string senderId, string receiverId)
        {
            try
            {
                if (chatDetail == null || string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
                {
                    Console.WriteLine($"Invalid SendMessage parameters: chatDetail={chatDetail}, senderId={senderId}, receiverId={receiverId}");
                    return;
                }

                var chat = await _chatRepository.GetChatByUsersAsync(senderId, receiverId);
                if (chat == null)
                {
                    Console.WriteLine($"Chat not found for senderId: {senderId}, receiverId: {receiverId}");
                    return;
                }

                var sender = await _accountRepository.GetAccountById(senderId);
                var receiver = await _accountRepository.GetAccountById(receiverId);
                if (sender == null || receiver == null)
                {
                    Console.WriteLine($"User not found: senderId={senderId}, receiverId={receiverId}");
                    return;
                }

                var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(senderId, receiverId);
                var unreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == receiverId);

                // ChatDTO cho người gửi
                var senderChatDTO = new ChatDTO
                {
                    ChatId = chat.ChatId,
                    Receiver = _mapper.Map<MyProfileDTO>(receiver),
                    LastMessageAccId = chatDetail.SenderId ?? "",
                    LastMessage = chatDetail.Message ?? (chatDetail.FileName != null ? "File: " + chatDetail.FileName : "No messages yet"),
                    LastMessageAt = chatDetail.SendAt,
                    UnreadCount = 0
                };

                // ChatDTO cho người nhận
                var receiverChatDTO = new ChatDTO
                {
                    ChatId = chat.ChatId,
                    Receiver = _mapper.Map<MyProfileDTO>(sender),
                    LastMessageAccId = chatDetail.SenderId ?? "",
                    LastMessage = chatDetail.Message ?? (chatDetail.FileName != null ? "File: " + chatDetail.FileName : "No messages yet"),
                    LastMessageAt = chatDetail.SendAt,
                    UnreadCount = unreadCount
                };

                await Clients.User(senderId).SendAsync("ReceiveMessage", chatDetail, senderChatDTO);
                await Clients.User(receiverId).SendAsync("ReceiveMessage", chatDetail, receiverChatDTO);
                Console.WriteLine($"Sent ReceiveMessage: chatId={chat.ChatId}, senderId={senderId}, receiverId={receiverId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// This method is called to notify the users about the deletion of chat history.
        /// It sends a notification to the users involved in the chat.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="accId1"></param>
        /// <param name="accId2"></param>
        /// <returns></returns>
        public async Task ChatHistoryDeleted(string chatId, string accId1, string accId2)
        {
            // Gửi sự kiện ChatHistoryDeleted với chatId
            await Clients.Users(new[] { accId1, accId2 }).SendAsync("ChatHistoryDeleted", chatId);
        }

        /// <summary>
        /// Broadcasts a "MessageSeen" event to all active connections of both participants in a chat,
        /// notifying them that messages in the specified chat have been seen.
        /// </summary>
        /// <param name="chatId">The ID of the chat whose message seen status is to be broadcasted.</param>
        public async Task BroadcastMessageSeen(string chatId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null) return;

            var otherUserId = chat.Acc1Id; // Hoặc Acc2Id, tùy vào người nhận
            var receiver = await _accountRepository.GetAccountById(otherUserId);
            if (receiver == null) return;

            var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(chat.Acc1Id, chat.Acc2Id);
            var unreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == otherUserId);

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

            await Clients.Users(new[] { chat.Acc1Id, chat.Acc2Id }).SendAsync("MessageSeen", chatId, chatDTO);
        }

        /// <summary>
        /// This method is called to notify users that a message has been revoked.
        /// </summary>
        /// <param name="chatId">The ID of the chat</param>
        /// <param name="accId1">The first user's ID</param>
        /// <param name="accId2">The second user's ID</param>
        /// <param name="chatDetailId">The ID of the chat message being recalled</param>
        /// <returns></returns>
        public async Task ChatRecalled(string chatId, string accId1, string accId2, string chatDetailId)
        {
            try
            {
                if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(accId1) || string.IsNullOrEmpty(accId2) || string.IsNullOrEmpty(chatDetailId))
                {
                    Console.WriteLine($"Invalid ChatRecalled parameters: chatId={chatId}, accId1={accId1}, accId2={accId2}, chatDetailId={chatDetailId}");
                    return;
                }

                var chat = await _chatRepository.GetChatByIdAsync(chatId);
                if (chat == null)
                {
                    Console.WriteLine($"Chat not found for chatId: {chatId}");
                    return;
                }

                var receiverId = accId1 == chat.Acc1Id ? chat.Acc2Id : chat.Acc1Id;
                var receiver = await _accountRepository.GetAccountById(receiverId);
                if (receiver == null)
                {
                    Console.WriteLine($"Receiver not found for receiverId: {receiverId}");
                    return;
                }

                var chatDetails = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(accId1, accId2);
                var lastMessage = chatDetails
                    .Where(c => c.IsRecalled != true) // Handle nullable boolean
                    .OrderByDescending(c => c.SendAt)
                    .FirstOrDefault();

                var chatDTO = new ChatDTO
                {
                    ChatId = chat.ChatId,
                    Receiver = _mapper.Map<MyProfileDTO>(receiver),
                    LastMessageAccId = lastMessage?.SenderId ?? "",
                    LastMessage = lastMessage?.Message ?? (lastMessage?.FileName != null ? "File: " + lastMessage.FileName : "No messages yet"),
                    LastMessageAt = lastMessage?.SendAt,
                    UnreadCount = chatDetails.Count(c => c.IsSeen != true && c.ReceiverId == receiverId)
                };

                if (chatDTO == null)
                {
                    Console.WriteLine("Failed to create chatDTO for ChatRecalled");
                    return;
                }

                await Clients.Users(new[] { accId1, accId2 }).SendAsync("ChatRecalled", chatId, chatDetailId, chatDTO);
                Console.WriteLine($"Sent ChatRecalled: chatId={chatId}, chatDetailId={chatDetailId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChatRecalled: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// This method is called to notify the recipient that the sender is typing.
        /// It sends the "ReceiveTypingNotification" to the recipient.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public async Task SendTyping(string senderId, string receiverId)
        {
            if (_accConnections.ContainsKey(receiverId))
            {
                var receiverConnections = _accConnections[receiverId];
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("SendTyping", senderId);
                }
            }
        }

        /// <summary>
        /// This method is called to notify the recipient that the sender has stopped typing.
        /// It sends the "StopTyping" to the recipient.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public async Task StopTyping(string senderId, string receiverId)
        {
            // Check if the receiver is connected.
            if (_accConnections.ContainsKey(receiverId))
            {
                var receiverConnections = _accConnections[receiverId];

                // Loop through each connection ID for the receiver and send the stop typing notification.
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("StopTyping", senderId);
                }
            }
        }
    }
}
