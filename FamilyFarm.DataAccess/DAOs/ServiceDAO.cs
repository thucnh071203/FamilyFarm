using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ServiceDAO:SingletonBase
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
            return await _Services.Find(s => s.IsDeleted != true && s.Status == 1).ToListAsync();
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
        public async Task<Service> GetByIdAsync(string serviceId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return null;

            return await _Services.Find(g => g.ServiceId == serviceId && g.IsDeleted != true).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Add new service
        /// </summary>
        public async Task<Service> CreateAsync(Service service)
        {
            service.ServiceId = ObjectId.GenerateNewId().ToString();
            service.CreateAt = DateTime.UtcNow;
            service.UpdateAt = null;
            //service.Status = 1;
            service.AverageRate = 0;
            service.RateCount = 0;
            service.IsDeleted = false;
            service.HaveProcess = false;

            await _Services.InsertOneAsync(service);
            return service;
        }

        /// <summary>
        ///     Update existing service
        /// </summary>
        public async Task<Service> UpdateAsync(string serviceId, Service item)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return null;

            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId) &
                         Builders<Service>.Filter.Eq(s => s.IsDeleted, false);

            if (filter == null) return null;

            var update = Builders<Service>.Update
                .Set(s => s.ServiceName, item.ServiceName)
                .Set(s => s.CategoryServiceId, item.CategoryServiceId)
                .Set(s => s.ServiceDescription, item.ServiceDescription)
                .Set(s => s.Price, item.Price)
                .Set(s => s.ImageUrl, item.ImageUrl)
                .Set(s => s.Status, item.Status)
                .Set(s => s.AverageRate, item.AverageRate)
                .Set(s => s.RateCount, item.RateCount)
                .Set(s => s.UpdateAt, DateTime.UtcNow);

            var result = await _Services.UpdateOneAsync(filter, update);

            var updatedService = await _Services.Find(g => g.ServiceId == serviceId && g.IsDeleted != true).FirstOrDefaultAsync();

            return updatedService;
        }

        public async Task<long> ChangeStatusAsync(string serviceId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return 0;

            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId) &
                         Builders<Service>.Filter.Eq(s => s.IsDeleted, false);

            var checkServiceStatus = await GetByIdAsync(serviceId);

            if (checkServiceStatus == null)
                return 0;

            UpdateDefinition<Service> update;

            if (checkServiceStatus.Status == 0)
            {
                update = Builders<Service>.Update
                .Set(s => s.Status, 1);
            } else
            {
                update = Builders<Service>.Update
                .Set(s => s.Status, 0);
            }

            var result = await _Services.UpdateOneAsync(filter, update);
            return result.ModifiedCount;
        }

        /// <summary>
        ///     Delete service
        /// </summary>
        public async Task<long> DeleteAsync(string serviceId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return 0;

            var filter = Builders<Service>.Filter.Eq(s => s.ServiceId, serviceId);
            var update = Builders<Service>.Update
                .Set(s => s.IsDeleted, true);

            var result = await _Services.UpdateOneAsync(filter, update);
            return result.ModifiedCount;
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

        public async Task UpdateStatus(string? serviceId, int status)
        {
            if (string.IsNullOrEmpty(serviceId))
                return;

            var filter = Builders<Service>.Filter.Eq(p => p.ServiceId, serviceId);
            var update = Builders<Service>.Update.Set(p => p.Status, status);

            await _Services.UpdateOneAsync(filter, update);
        }

        public async Task<Service> GetLastestServiceByProviderAsync(string serviceId, string accId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return null;

            if (!ObjectId.TryParse(accId, out _)) return null;

            var filter = Builders<Service>.Filter.Eq(p => p.ServiceId, serviceId) &
                         Builders<Service>.Filter.Eq(p => p.ProviderId, accId) &
                         Builders<Service>.Filter.Ne(p => p.IsDeleted, true);

            var sort = Builders<Service>.Sort.Descending(s => s.CreateAt);

            var latestService = await _Services
                .Find(filter)
                .Sort(sort)
                .Limit(1)
                .FirstOrDefaultAsync();

            return latestService;
        }

        public async Task UpdateProcessStatus(string? serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return;

            var filter = Builders<Service>.Filter.Eq(p => p.ServiceId, serviceId);
            var update = Builders<Service>.Update.Set(p => p.HaveProcess, true);

            await _Services.UpdateOneAsync(filter, update);
        }

        /// <summary>
        ///     Get service by Id
        /// </summary>
        public async Task<Service> GetByIdOutDeleteAsync(string serviceId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return null;

            return await _Services.Find(g => g.ServiceId == serviceId).FirstOrDefaultAsync();
        }
    }
}
