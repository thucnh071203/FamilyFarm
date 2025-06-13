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
            // Lấy accId từ claim "AccId" (khớp với AuthenticationService)
            var accId = Context.User?.FindFirst("accId")?.Value;

            Console.WriteLine($"User connected with claims: {string.Join(", ", Context.User?.Claims.Select(c => $"{c.Type}: {c.Value}") ?? new List<string>())}");
            Console.WriteLine($"User connected with accId: {accId ?? "null"}");

            if (!string.IsNullOrEmpty(accId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, accId);
                Console.WriteLine($"Added to group: {accId}");
            }
            else
            {
                Console.WriteLine("No accId found in user claims");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var accId = Context.User?.FindFirst("accId")?.Value;
            Console.WriteLine($"User disconnected with accId: {accId ?? "null"}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string accId, Notification notification)
        {
            Console.WriteLine($"Sending notification to group {accId}: {Newtonsoft.Json.JsonConvert.SerializeObject(notification)}");
            await Clients.Group(accId).SendAsync("ReceiveNotification", notification);
        }
    }
}