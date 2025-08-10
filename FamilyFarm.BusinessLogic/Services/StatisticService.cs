using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;

        public StatisticService(IStatisticRepository statisticRepository, IHubContext<TopEngagedPostHub> hubContext)
        {
            _statisticRepository = statisticRepository;
            _hubContext = hubContext;
        }

        public async Task<List<EngagedPostResponseDTO>> GetTopEngagedPostsAsync(int topN)
        {
            return await _statisticRepository.GetTopEngagedPostsAsync(topN);
        }

        public async Task<Dictionary<string, int>> GetWeeklyBookingGrowthAsync()
        {
            return await _statisticRepository.GetWeeklyBookingGrowthAsync();
        }

        public async Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate)
        {
            return await _statisticRepository.GetMostActiveMembersAsync(startDate, endDate);
        }

        public async Task<List<UserByProvinceResponseDTO>> GetUsersByProvinceAsync()
        {
            return await _statisticRepository.GetUsersByProvinceAsync();
        }

        public async Task<Dictionary<string, int>> GetCountByDateAsync(string accId, string time)
        {
            return await _statisticRepository.GetCountByDateAsync(accId, time);
        }

        public async Task<Dictionary<string, int>> GetCountByMonthAsync(string accId, int year)
        {
            return await _statisticRepository.GetCountByMonthAsync(accId, year);
        }

        public async Task<Dictionary<string, int>> GetCountByDayAllMonthsAsync(string accId, int year)
        {
            return await _statisticRepository.GetCountByDayAllMonthsAsync(accId, year);
        }
        public async Task<Dictionary<string, int>> GetPopularServiceCategoriesAsync(string accId)
        {
            return await _statisticRepository.GetPopularServiceCategoriesAsync(accId);
        }
        public async Task<Dictionary<string, int>> GetMostBookedServicesByExpertAsync(string accId)
        {
            return await _statisticRepository.GetMostBookedServicesByExpertAsync(accId);
        }
        public async Task<ExpertRevenueDTO> GetRevenueByExpertAsync(string expertId, DateTime? from = null, DateTime? to = null)
        {

            var revenue = await _statisticRepository.GetExpertRevenueAsync(expertId, from, to);

            await _hubContext.Clients.All.SendAsync("ReceiveRevenueUpdate", revenue);

            return revenue;
        }

        public async Task<RevenueSystemDTO> GetSystemRevenueAsync(DateTime? from = null, DateTime? to = null)
        {
            var revenue = await _statisticRepository.GetSystemRevenueAsync(from, to);

            await _hubContext.Clients.All.SendAsync("ReceiveRevenueUpdate", revenue);

            return revenue;
        }

        public async Task<List<BookingService>> GetBookingsByStatusAsync(string accId, string status)
        {
            return await _statisticRepository.GetBookingsByStatusAsync(accId, status);
        }
        public async Task<long> GetTotalPostCountAsync()
        {
            return await _statisticRepository.CountPostsAsync();
        }
    }
}
