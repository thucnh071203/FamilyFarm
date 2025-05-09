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
    public class NotificationStatusRepository : INotificationStatusRepository
    {
        private readonly NotificationStatusDAO _notificationStatusDAO;

        public NotificationStatusRepository(NotificationStatusDAO notificationStatusDAO)
        {
            _notificationStatusDAO = notificationStatusDAO;
        }

        public async Task<NotificationStatus> GetByIdAsync(string notifiStatusId)
        {
            return await _notificationStatusDAO.GetByIdAsync(notifiStatusId);
        }

        public async Task<NotificationStatus> GetByAccAndNotifiAsync(string accId, string notifiId)
        {
            return await _notificationStatusDAO.GetByAccAndNotifiAsync(accId, notifiId);
        }

        public async Task CreateAsync(NotificationStatus status)
        {
            await _notificationStatusDAO.CreateAsync(status);
        }

        public async Task CreateManyAsync(List<NotificationStatus> statuses)
        {
            await _notificationStatusDAO.CreateManyAsync(statuses);
        }

        public async Task<List<NotificationStatus>> GetByReceiverIdAsync(string receiverId)
        {
            return await _notificationStatusDAO.GetByReceiverIdAsync(receiverId);
        }

        public async Task<NotificationStatus> UpdateAsync(NotificationStatus status)
        {
            return await _notificationStatusDAO.UpdateAsync(status);
        }

        public async Task<bool> MarkAllAsReadByNotifiIdAsync(string notifiStatusId)
        {
            return await _notificationStatusDAO.MarkAllAsReadByNotifiIdAsync(notifiStatusId);
        }

        public async Task<bool> MarkAllAsReadByAccIdAsync(string accId)
        {
            return await _notificationStatusDAO.MarkAllAsReadByAccIdAsync(accId);
        }
    }
}
