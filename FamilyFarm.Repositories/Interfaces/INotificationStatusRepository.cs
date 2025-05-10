using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface INotificationStatusRepository
    {
        Task<NotificationStatus> GetByIdAsync(string notifiStatusId);
        Task<NotificationStatus> GetByAccAndNotifiAsync(string accId, string notifiId);
        Task CreateAsync(NotificationStatus status);
        Task CreateManyAsync(List<NotificationStatus> statuses);
        Task<List<NotificationStatus>> GetByReceiverIdAsync(string receiverId);
        Task<NotificationStatus> UpdateAsync(NotificationStatus status);
        Task<bool> MarkAllAsReadByNotifiIdAsync(string notifiStatusId);
        Task<bool> MarkAllAsReadByAccIdAsync(string accId);
    }
}
