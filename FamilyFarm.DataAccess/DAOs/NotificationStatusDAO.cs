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
    public class NotificationStatusDAO
    {
        private readonly IMongoCollection<NotificationStatus> _notificationStatuses;

        public NotificationStatusDAO(IMongoDatabase database)
        {
            _notificationStatuses = database.GetCollection<NotificationStatus>("NotificationStatus");
        }

        public async Task<NotificationStatus> GetByIdAsync(string notifiStatusId)
        {
            return await _notificationStatuses
                .Find(n => n.NotifiStatusId == notifiStatusId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(NotificationStatus status)
        {
            await _notificationStatuses.InsertOneAsync(status);
        }

        public async Task CreateManyAsync(List<NotificationStatus> statuses)
        {
            await _notificationStatuses.InsertManyAsync(statuses);
        }

        public async Task<List<NotificationStatus>> GetByReceiverIdAsync(string receiverId)
        {
            if (!ObjectId.TryParse(receiverId, out _))
                return new List<NotificationStatus>();

            return await _notificationStatuses
                .Find(s => s.AccId == receiverId)
                .ToListAsync();
        }

        public async Task<NotificationStatus> UpdateAsync(NotificationStatus status)
        {
            var result = await _notificationStatuses
                .ReplaceOneAsync(s => s.NotifiStatusId == status.NotifiStatusId, status);
            return result.MatchedCount > 0 ? status : null;
        }

        public async Task<bool> MarkAllAsReadByNotifiIdAsync(string notifiId)
        {
            var update = Builders<NotificationStatus>.Update.Set(s => s.IsRead, true);
            var result = await _notificationStatuses
                .UpdateOneAsync(s => s.NotifiStatusId == notifiId, update);
            return true;
        }

        public async Task<bool> MarkAllAsReadByAccIdAsync(string accId)
        {
            var update = Builders<NotificationStatus>.Update.Set(s => s.IsRead, true);
            var result = await _notificationStatuses
                .UpdateManyAsync(s => s.AccId == accId, update);
            return true;
        }
    }
}
