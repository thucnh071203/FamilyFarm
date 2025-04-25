using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ServiceDAO
    {
        private readonly IMongoCollection<Service> _Services;

        public ServiceDAO(IMongoDatabase database)
        {
            _Services = database.GetCollection<Service>("Service");
        }

        /// <summary>
        ///     Get a list of all available services
        /// </summary>
        public async Task<List<Service>> GetAllAsync()
        {
            return await _Services.Find(s => s.IsDeleted != true).ToListAsync();
        }

        /// <summary>
        ///     Get list of all available services by provider Id
        /// </summary>
        public async Task<List<Service>> GetAllByProviderIdAsync(string providerId)
        {
            var filter = Builders<Service>.Filter.Eq(s => s.ProviderId, providerId) &
                         Builders<Service>.Filter.Ne(s => s.IsDeleted, true);

            return await _Services.Find(filter).ToListAsync();
        }

        /// <summary>
        ///     Get service by Id
        /// </summary>
        public async Task<Service?> GetByIdAsync(string serviceId)
        {
            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId) &
                         Builders<Service>.Filter.Ne(s => s.IsDeleted, true);

            return await _Services.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Add new service
        /// </summary>
        public async Task<Service> InsertAsync(Service item)
        {
            await _Services.InsertOneAsync(item);
            return item;
        }

        /// <summary>
        ///     Update existing service
        /// </summary>
        public async Task<bool> UpdateAsync(string serviceId, Service item)
        {
            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId);

            var update = Builders<Service>.Update
                .Set(s => s.ServiceName, item.ServiceName)
                .Set(s => s.ServiceDescription, item.ServiceDescription)
                .Set(s => s.Price, item.Price)
                .Set(s => s.ImageUrl, item.ImageUrl)
                .Set(s => s.CategoryServiceId, item.CategoryServiceId)
                .Set(s => s.UpdateAt, item.UpdateAt);

            var result = await _Services.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }

        /// <summary>
        ///     Delete service
        /// </summary>
        public async Task<bool> DeleteAsync(string serviceId)
        {
            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId);
            var update = Builders<Service>.Update
                .Set(s => s.IsDeleted, true);

            var result = await _Services.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        ///     Get list of available services by pagination
        /// </summary>
        public async Task<List<Service>> GetAllPagPageAsync(int pageNumber, int pageSize)
        {
            var filter = Builders<Service>.Filter.Ne(s => s.IsDeleted, true);

            return await _Services.Find(filter)
                .SortBy(s => s.ServiceId) // Đảm bảo thứ tự ổn định
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <summary>
        ///     Count total number of available services
        /// </summary>
        public async Task<long> GetTotalAllCountAsync()
        {
            var filter = Builders<Service>.Filter.Ne(s => s.IsDeleted, true);
            return await _Services.CountDocumentsAsync(filter);
        }
    }
}
