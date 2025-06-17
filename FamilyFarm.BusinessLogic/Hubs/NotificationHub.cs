using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Lấy accId từ JWT claims thay vì query parameter
            var accId = Context.User?.FindFirst("AccId")?.Value
                       ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? Context.User?.FindFirst("sub")?.Value;


            if (!string.IsNullOrEmpty(accId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, accId);
            }
            else
            {
                Console.WriteLine("No accId found in user claims");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var accId = Context.User?.FindFirst("AccId")?.Value
                       ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? Context.User?.FindFirst("sub")?.Value;
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string accId, Notification notification)
        {
            await Clients.Group(accId).SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyGroupUpdated(string accId, Group group)
        {
            await Clients.Group(accId).SendAsync("GroupUpdated", group);
        }
    }
}