using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface INotificationService
    {
        Task<SendNotificationResponseDTO> SendNotificationAsync(SendNotificationRequestDTO request);
        Task<ListNotifiResponseDTO> GetNotificationsForUserAsync(string accId);
        Task<bool> MarkAsReadByNotificationIdAsync(string notificationId);
        Task<bool> MarkAllAsReadByAccIdAsync(string accId);
    }
}
