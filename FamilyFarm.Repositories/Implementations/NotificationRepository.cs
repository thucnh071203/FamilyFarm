using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDAO;

        public NotificationRepository(NotificationDAO notificationDAO)
        {
            _notificationDAO = notificationDAO;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            return await _notificationDAO.CreateAsync(notification);
        }

        public async Task<Notification> GetByIdAsync(string notificationId)
        {
            return await _notificationDAO.GetByIdAsync(notificationId);
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            return await _notificationDAO.UpdateAsync(notification);
        }

        public async Task<List<Notification>> GetByNotifiIdsAsync(List<string> notifiIds)
        {
            return await _notificationDAO.GetByNotifiIdsAsync(notifiIds);
        }
    }
}
