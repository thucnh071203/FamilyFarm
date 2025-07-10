using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class RevenueRepository : IRevenueRepository
    {
        private readonly RevenueDAO _dao;
        public RevenueRepository(RevenueDAO dao)
        {
            _dao = dao;
        }
        public async Task<Revenue> CreateNewRevenue()
        {
            return await _dao.GetOrCreateRevenueAsync();
        }
        public async Task<bool> ChangeRevenue(decimal? total, decimal? commission)
        {
            return await _dao.ChangeRevenueAsync(total, commission);
        }
    }
}
