using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> GetByIdAsync(string notificationId);
        Task<Notification> UpdateAsync(Notification notification);
        Task<List<Notification>> GetByNotifiIdsAsync(List<string> notifiIds);
    }
}
