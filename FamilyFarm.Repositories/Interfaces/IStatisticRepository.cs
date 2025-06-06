﻿using FamilyFarm.Models.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IStatisticRepository
    {
        Task<List<EngagedPostResponseDTO>> GetTopEngagedPostsAsync(int topN);
        Task<Dictionary<string, int>> GetWeeklyBookingGrowthAsync();
        Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate);
        Task<List<UserByProvinceResponseDTO>> GetUsersByProvinceAsync();
    }
}
