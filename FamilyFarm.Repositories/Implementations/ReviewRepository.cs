using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewDAO _reviewDAO;

        public ReviewRepository(ReviewDAO reviewDAO)
        {
            _reviewDAO = reviewDAO;
        }

        public async Task<List<Review>> GetByServiceIdAsync(string serviceId)
        {
            return await _reviewDAO.GetByServiceIdAsync(serviceId);
        }

        public async Task<Review> GetByIdAsync(string id)
        {
            return await _reviewDAO.GetByIdAsync(id);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            return await _reviewDAO.CreateAsync(review);
        }

        public async Task<Review> UpdateAsync(string id, Review review)
        {
            return await _reviewDAO.UpdateAsync(id, review);
        }

        public async Task DeleteAsync(string id)
        {
            await _reviewDAO.DeleteAsync(id);
        }
    }
}
