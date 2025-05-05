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
    public class NotificationDAO
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationDAO(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("Notification");
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        public async Task<Notification> GetByIdAsync(string notificationId)
        {
            return await _notifications
                .Find(n => n.NotifiId == notificationId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Notification>> GetByNotifiIdsAsync(List<string> notifiIds)
        {
            return await _notifications
                .Find(n => notifiIds.Contains(n.NotifiId))
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            var result = await _notifications
                .ReplaceOneAsync(n => n.NotifiId == notification.NotifiId, notification);
            return result.MatchedCount > 0 ? notification : null;
        }
    }
}
