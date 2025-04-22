using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetAllAsync();
        Task<List<Service>> GetAllByProviderIdAsync(ObjectId providerId);
        Task<Service?> GetByIdAsync(ObjectId serviceId);
        Task<Service> InsertAsync(Service item);
        Task<bool> UpdateAsync(ObjectId serviceId, Service item);
        Task<bool> DeleteAsync(ObjectId serviceId);
        Task<List<Service>> GetAllPagPageAsync(int pageNumber, int pageSize);
        Task<long> GetTotalAllCountAsync();
    }
}
