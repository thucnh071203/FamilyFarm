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
        Task<List<Service>> GetAllByProviderIdAsync(string providerId);
        Task<Service?> GetByIdAsync(string serviceId);
        //Task<Service> InsertAsync(Service item);
        //Task<bool> UpdateAsync(string serviceId, Service item);
        //Task<bool> DeleteAsync(string serviceId);
        //Task<List<Service>> GetAllPagPageAsync(int pageNumber, int pageSize);
        //Task<long> GetTotalAllCountAsync();
    }
}
