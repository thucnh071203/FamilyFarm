using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Repositories.Interfaces;
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
    }
}
