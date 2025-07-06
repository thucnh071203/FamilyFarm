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
        Task<List<Service>> GetAllService();
        Task<List<Service>> GetAllServiceByProvider(string providerId);
        Task<Service> GetServiceById(string serviceId);
        Task<Service> CreateService(Service service);
        Task<Service> UpdateService(string serviceId, Service service);
        Task<long> ChangeStatusService(string serviceId);
        Task UpdateStatusService(string? serviceId, int status);
        Task<long> DeleteService(string serviceId);
        //Task<List<Service>> GetAllPagPageAsync(int pageNumber, int pageSize);
        //Task<long> GetTotalAllCountAsync();
        Task<Service> GetLastestService(string serviceId, string accId);
        Task UpdateProcessStatusService(string? serviceId);
        Task<Service> GetByIdOutDelete(string serviceId);
    }
}
