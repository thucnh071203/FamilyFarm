using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class BookingHub : Hub
    {

        // Gửi tới người dùng cụ thể (hoặc broadcast tùy ý)
        public async Task SendBookingStatusChanged(string bookingId, string newStatus)
        {
            await Clients.All.SendAsync("ReceiveBookingStatusChanged", bookingId, newStatus);
        }
    }
}
