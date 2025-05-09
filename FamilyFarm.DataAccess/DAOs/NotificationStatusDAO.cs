using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class NotificationStatusDAO
    {
        private readonly IMongoCollection<NotificationStatus> _notificationStatuses;

        // Constructor to initialize the NotificationStatus collection from the MongoDB database
        public NotificationStatusDAO(IMongoDatabase database)
        {
            _notificationStatuses = database.GetCollection<NotificationStatus>("NotificationStatus");
        }

        /// <summary>
        /// Retrieves a NotificationStatus by its unique ID.
        /// </summary>
        /// <param name="notifiStatusId">The ID of the notification status.</param>
        /// <returns>The matching NotificationStatus or null if not found.</returns>
        public async Task<NotificationStatus> GetByIdAsync(string notifiStatusId)
        {
            return await _notificationStatuses
                .Find(n => n.NotifiStatusId == notifiStatusId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="notifiId"></param>
        /// <returns></returns>
        public async Task<NotificationStatus> GetByAccAndNotifiAsync(string accId, string notifiId)
        {
            return await _notificationStatuses
                .Find(n => n.NotifiId == notifiId && n.AccId == accId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts a new NotificationStatus document into the database.
        /// </summary>
        /// <param name="status">The NotificationStatus object to insert.</param>
        public async Task CreateAsync(NotificationStatus status)
        {
            await _notificationStatuses.InsertOneAsync(status);
        }

        /// <summary>
        /// Inserts multiple NotificationStatus documents into the database.
        /// </summary>
        /// <param name="statuses">A list of NotificationStatus objects to insert.</param>
        public async Task CreateManyAsync(List<NotificationStatus> statuses)
        {
            await _notificationStatuses.InsertManyAsync(statuses);
        }

        /// <summary>
        /// Retrieves all NotificationStatus documents associated with a specific receiver/account ID.
        /// </summary>
        /// <param name="receiverId">The ID of the receiver/account.</param>
        /// <returns>A list of NotificationStatus objects.</returns>
        public async Task<List<NotificationStatus>> GetByReceiverIdAsync(string receiverId)
        {
            if (!ObjectId.TryParse(receiverId, out _))
                return new List<NotificationStatus>();

            return await _notificationStatuses
                .Find(s => s.AccId == receiverId)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing NotificationStatus document in the database.
        /// </summary>
        /// <param name="status">The NotificationStatus object with updated data.</param>
        /// <returns>The updated NotificationStatus if successful; otherwise, null.</returns>
        public async Task<NotificationStatus> UpdateAsync(NotificationStatus status)
        {
            var result = await _notificationStatuses
                .ReplaceOneAsync(s => s.NotifiStatusId == status.NotifiStatusId, status);
            return result.MatchedCount > 0 ? status : null;
        }

        /// <summary>
        /// Marks a specific notification as read based on its notification ID.
        /// </summary>
        /// <param name="notifiId">The ID of the notification to mark as read.</param>
        /// <returns>True if the update is successful.</returns>
        public async Task<bool> MarkAllAsReadByNotifiIdAsync(string notifiId)
        {
            var update = Builders<NotificationStatus>.Update.Set(s => s.IsRead, true);
            var result = await _notificationStatuses
                .UpdateOneAsync(s => s.NotifiStatusId == notifiId, update);
            return true;
        }

        /// <summary>
        /// Marks all notifications as read for a specific account ID.
        /// </summary>
        /// <param name="accId">The account ID whose notifications will be marked as read.</param>
        /// <returns>True if the update is successful.</returns>
        public async Task<bool> MarkAllAsReadByAccIdAsync(string accId)
        {
            var update = Builders<NotificationStatus>.Update.Set(s => s.IsRead, true);
            var result = await _notificationStatuses
                .UpdateManyAsync(s => s.AccId == accId, update);
            return true;
        }
    }
}
