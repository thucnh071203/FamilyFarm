using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Sends a real-time notification to a specific user via SignalR.
        /// </summary>
        /// <param name="accId">The ID of the user to receive the notification.</param>
        /// <param name="notification">The notification object to send.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendNotification(string accId, Notification notification)
        {
            await Clients.User(accId).SendAsync("ReceiveNotification", notification);
        }
    }
}
