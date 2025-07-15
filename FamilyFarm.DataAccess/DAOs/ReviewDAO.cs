using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ReviewDAO : SingletonBase
    {
        private readonly IMongoCollection<Review> _Review;
        public ReviewDAO(IMongoDatabase database)
        {
            _Review = database.GetCollection<Review>("ReviewService");
        }

        public async Task<List<Review>> GetByServiceIdAsync(string serviceId)
        {
            return await _Review.Find(r => r.ServiceId == serviceId && !r.IsDeleted).ToListAsync();
        }

        public async Task<Review> GetByIdAsync(string id)
        {
            return await _Review.Find(r => r.ReviewId == id && !r.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<Review> CreateAsync(Review review)
        {
            await _Review.InsertOneAsync(review);
            return review;
        }

        public async Task<Review> UpdateAsync(string id, Review review)
        {
            await _Review.ReplaceOneAsync(r => r.ReviewId == id, review);
            return await _Review.Find(r => r.ReviewId == id).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var update = Builders<Review>.Update
                .Set(r => r.IsDeleted, true)
                .Set(r => r.DeletedAt, DateTime.UtcNow);
            await _Review.UpdateOneAsync(r => r.ReviewId == id, update);
        }
    }
}
