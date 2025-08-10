using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly StatisticDAO _statisticDAO;
        private readonly ReactionDAO _reactionDAO;
        private readonly CommentDAO _commentDAO;

        public StatisticRepository(StatisticDAO statisticDAO, ReactionDAO reactionDAO, CommentDAO commentDAO)
        {
            _statisticDAO = statisticDAO;
            _reactionDAO = reactionDAO;
            _commentDAO = commentDAO;
        }

        public async Task<List<EngagedPostResponseDTO>> GetTopEngagedPostsAsync(int topN)
        {
            return await _statisticDAO.GetTopEngagedPostsAsync(topN, _reactionDAO, _commentDAO);
        }

        public async Task<Dictionary<string, int>> GetWeeklyBookingGrowthAsync()
        {
            return await _statisticDAO.GetWeeklyBookingGrowthAsync();
        }

        public async Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate)
        {
            return await _statisticDAO.GetMostActiveMembersAsync(startDate, endDate);
        }

        public async Task<List<UserByProvinceResponseDTO>> GetUsersByProvinceAsync()
        {
            return await _statisticDAO.GetUsersByProvinceAsync();
        }

        public async Task<Dictionary<string, int>> GetCountByDateAsync(string accId, string time)
        {
            return await _statisticDAO.CountByDateAsync(accId, time);
        }

        public async Task<Dictionary<string, int>> GetCountByMonthAsync(string accId, int year)
        {
            return await _statisticDAO.GetCountByMonthAsync(accId, year);
        }

        public async Task<Dictionary<string, int>> GetCountByDayAllMonthsAsync(string accId, int year)
        {
            return await _statisticDAO.GetCountByDayAllMonthsAsync(accId, year);
        }
        public async Task<Dictionary<string, int>> GetPopularServiceCategoriesAsync(string accId)
        {
            return await _statisticDAO.GetPopularServiceCategoriesAsync(accId);
        }

        public async Task<Dictionary<string, int>> GetMostBookedServicesByExpertAsync(string accId)
        {
            return await _statisticDAO.GetMostBookedServicesByExpertAsync(accId);
        }

        public async Task<ExpertRevenueDTO> GetExpertRevenueAsync(string expertId, DateTime? from = null, DateTime? to = null)
        {
            return await _statisticDAO.GetExpertRevenueAsync(expertId, from, to);
        }
        public async Task<RevenueSystemDTO> GetSystemRevenueAsync(DateTime? from = null, DateTime? to = null)
        {
            var revenue = await _statisticDAO.FindPaidBookingsAsync(from, to);
            return revenue;
        }


        public async Task<List<BookingService>> GetBookingsByStatusAsync(string accId, string status)
        {
            return await _statisticDAO.GetBookingsByStatusAsync(accId, status);
        }

        public async Task<long> CountPostsAsync()
        {
            return await _statisticDAO.CountPostsAsync();
        }
    }
}
