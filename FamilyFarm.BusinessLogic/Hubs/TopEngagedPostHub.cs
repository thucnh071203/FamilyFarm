using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Hubs
{
    public class TopEngagedPostHub : Hub
    {
        public async Task SendUpdatedTopPosts(List<EngagedPostResponseDTO> posts)
        {
            await Clients.All.SendAsync("topEngagedPostHub", posts);
        }

    }
}
