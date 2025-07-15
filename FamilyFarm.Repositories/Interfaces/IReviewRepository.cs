using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByServiceIdAsync(string serviceId);
        Task<Review> GetByIdAsync(string id);
        Task<Review> CreateAsync(Review review);
        Task<Review> UpdateAsync(string id, Review review);
        Task DeleteAsync(string id);
    }
}
