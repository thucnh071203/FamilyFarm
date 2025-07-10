using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IRevenueRepository
    {
        Task<Revenue> CreateNewRevenue();
        Task<bool> ChangeRevenue(decimal? total, decimal? commission);
    }
}
