using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class FriendHub:Hub
    {
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Log hoặc xử lý nếu cần
                Console.WriteLine($"[SignalR] User connected: {userId}");
            }
            else
            {
                Console.WriteLine("[SignalR] User connected but no user ID found!");
            }

            return base.OnConnectedAsync();
        }
    }
}
