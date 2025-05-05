using FamilyFarm.Models.Models;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class NotificationDAO
    {
        private readonly IMongoCollection<Notification> _notifications;

        // Constructor to initialize the Notification collection from the MongoDB database
        public NotificationDAO(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("Notification");
        }

        /// <summary>
        /// Inserts a new Notification document into the database.
        /// </summary>
        /// <param name="notification">The Notification object to be created.</param>
        /// <returns>The created Notification object.</returns>
        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        /// <summary>
        /// Retrieves a Notification by its unique ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to retrieve.</param>
        /// <returns>The matching Notification object, or null if not found.</returns>
        public async Task<Notification> GetByIdAsync(string notificationId)
        {
            return await _notifications
                .Find(n => n.NotifiId == notificationId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a list of Notifications based on a list of notification IDs.
        /// The results are sorted in descending order by creation date.
        /// </summary>
        /// <param name="notifiIds">A list of notification IDs to fetch.</param>
        /// <returns>A list of matching Notification objects.</returns>
        public async Task<List<Notification>> GetByNotifiIdsAsync(List<string> notifiIds)
        {
            return await _notifications
                .Find(n => notifiIds.Contains(n.NotifiId))
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing Notification in the database.
        /// </summary>
        /// <param name="notification">The Notification object with updated data.</param>
        /// <returns>The updated Notification object if successful; otherwise, null.</returns>
        public async Task<Notification> UpdateAsync(Notification notification)
        {
            var result = await _notifications
                .ReplaceOneAsync(n => n.NotifiId == notification.NotifiId, notification);
            return result.MatchedCount > 0 ? notification : null;
        }
    }
}
