using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class ChatHub : Hub
    {
        // A static dictionary to store the connection IDs for each account.
        // The key is the account ID, and the value is a list of connection IDs associated with that account.
        private static readonly Dictionary<string, List<string>> _accConnections = new();

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
                Console.WriteLine("Connection rejected: Missing AccId in query string.");
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

                // Log the connection details.
                Console.WriteLine($"AccId {accId} connected. ConnectionId: {Context.ConnectionId}, Total connections: {_accConnections[accId].Count}");
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

                    // Log the disconnection details.
                    Console.WriteLine($"AccId {accId} disconnected. ConnectionId: {Context.ConnectionId}, Remaining connections: {_accConnections[accId].Count}");

                    // If there are no remaining connections for the account, remove the account from the dictionary.
                    if (_accConnections[accId].Count == 0)
                    {
                        _accConnections.Remove(accId);
                    }
                }
            }
            else
            {
                // If the connection ID is not associated with any account, log that information.
                Console.WriteLine($"Disconnected ConnectionId {Context.ConnectionId} not associated with any AccId.");
            }

            // Call the base method to complete the disconnection process.
            await base.OnDisconnectedAsync(exception);
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

            // Log the notification details.
            Console.WriteLine($"Notifying ChatHistoryDeleted for ChatId: {chatId}, AccIds: {accId1}, {accId2}");

            // Send the "ChatHistoryDeleted" notification to the target users.
            await Clients.Users(targetAccIds).SendAsync("ChatHistoryDeleted", chatId);
        }

        /// <summary>
        /// This method is called to notify users that a message has been revoked.
        /// </summary>
        /// <param name="chatId">The ID of the chat</param>
        /// <param name="accId1">The first user's ID</param>
        /// <param name="accId2">The second user's ID</param>
        /// <param name="chatDetailId">The ID of the chat message being revoked</param>
        /// <returns></returns>
        public async Task ChatRevoked(string chatId, string accId1, string accId2, string chatDetailId)
        {
            // Target both users in the chat to notify about the message revocation
            var targetAccIds = new[] { accId1, accId2 };

            // Log the details of the notification
            Console.WriteLine($"Notifying ChatRevoked for ChatId: {chatId}, ChatDetailId: {chatDetailId}, AccIds: {accId1}, {accId2}");

            // Send the "ChatRevoked" event to the users involved in the chat
            await Clients.Users(targetAccIds).SendAsync("ChatRevoked", chatId, chatDetailId);
        }

        /// <summary>
        /// This method is called to notify the recipient that the sender is typing.
        /// It sends the "ReceiveTypingNotification" to the recipient.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public async Task SendTypingNotification(string senderId, string receiverId)
        {
            // Check if the receiver is connected.
            if (_accConnections.ContainsKey(receiverId))
            {
                var receiverConnections = _accConnections[receiverId];

                // Loop through each connection ID for the receiver and send the typing notification.
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveTypingNotification", senderId);
                }
            }
        }

        /// <summary>
        /// This method is called to notify the recipient that the sender has stopped typing.
        /// It sends the "StopTypingNotification" to the recipient.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public async Task StopTypingNotification(string senderId, string receiverId)
        {
            // Check if the receiver is connected.
            if (_accConnections.ContainsKey(receiverId))
            {
                var receiverConnections = _accConnections[receiverId];

                // Loop through each connection ID for the receiver and send the stop typing notification.
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("StopTypingNotification", senderId);
                }
            }
        }
    }
}
