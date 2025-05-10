using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
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

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
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
            // Send the "ReceiveMessage" event to both the sender and receiver
            await Clients.Users(new[] { senderId, receiverId }).SendAsync("ReceiveMessage", chatDetail);

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
            // Define the target account IDs to notify.
            var targetAccIds = new[] { accId1, accId2 };

            // Send the "ChatHistoryDeleted" notification to the target users.
            await Clients.Users(targetAccIds).SendAsync("ChatHistoryDeleted", chatId);
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

            string accId1 = chat.Acc1Id;
            string accId2 = chat.Acc2Id;

            // Gửi sự kiện MessageSeen đến cả hai người dùng
            if (_accConnections.ContainsKey(accId1))
            {
                foreach (var connectionId in _accConnections[accId1])
                {
                    await Clients.Client(connectionId).SendAsync("MessageSeen", chatId);
                }
            }

            if (_accConnections.ContainsKey(accId2))
            {
                foreach (var connectionId in _accConnections[accId2])
                {
                    await Clients.Client(connectionId).SendAsync("MessageSeen", chatId);
                }
            }
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
            // Target both users in the chat to notify about the message revocation
            var targetAccIds = new[] { accId1, accId2 };

            // Send the "ChatRecalled" event to the users involved in the chat
            await Clients.Users(targetAccIds).SendAsync("ChatRecalled", chatId, chatDetailId);
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
