using FamilyFarm.Models.DTOs.EntityDTO;
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
    public interface IStatisticService
    {
        Task<List<EngagedPostResponseDTO>> GetTopEngagedPostsAsync(int topN);
        Task<Dictionary<string, int>> GetWeeklyBookingGrowthAsync();
        Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate);
        Task<List<UserByProvinceResponseDTO>> GetUsersByProvinceAsync();
        //Task<Dictionary<string, int>> GetCountByStatusAsync(string accId);
        //Task<Dictionary<string, List<BookingServiceByStatusDTO>>> GetCountByStatusAsync(string accId);
        Task<Dictionary<string, int>> GetCountByDateAsync(string accId, string time);
        Task<Dictionary<string, int>> GetCountByDayAllMonthsAsync(string accId, int year);
             Task<Dictionary<string, int>> GetCountByMonthAsync(string accId, int year);
        Task<Dictionary<string, int>> GetPopularServiceCategoriesAsync(string accId);
        Task<Dictionary<string, int>> GetMostBookedServicesByExpertAsync(string accId);
        Task<ExpertRevenueDTO> GetRevenueByExpertAsync(string expertId, DateTime? from = null, DateTime? to = null);
        Task<RevenueSystemDTO> GetSystemRevenueAsync(DateTime? from = null, DateTime? to = null);
        Task<List<BookingService>> GetBookingsByStatusAsync(string accId, string status);
        Task<long> GetTotalPostCountAsync();

    }
}
