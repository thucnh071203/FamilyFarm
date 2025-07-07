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
    public class ServiceRepository : IServiceRepository
    {
        private readonly ServiceDAO _dao;
        public ServiceRepository(ServiceDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<Service>> GetAllService()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<List<Service>> GetAllServiceByProvider(string providerId)
        {
            return await _dao.GetAllByProviderIdAsync(providerId);
        }

        public async Task<Service> GetServiceById(string serviceId)
        {
            return await _dao.GetByIdAsync(serviceId); 
        }

        public async Task<Service> CreateService(Service item)
        {
            return await _dao.CreateAsync(item);
        }

        public async Task<Service> UpdateService(string serviceId, Service item)
        {
            return await _dao.UpdateAsync(serviceId, item);
        }

        public async Task<long> ChangeStatusService(string serviceId)
        {
            return await _dao.ChangeStatusAsync(serviceId);
        }

        public async Task<long> DeleteService(string serviceId)
        {
            return await _dao.DeleteAsync(serviceId);
        }

        public async Task UpdateStatusService(string? serviceId, int status)
        {
            await _dao.UpdateStatus(serviceId, status);
        }

        public async Task<Service> GetLastestService(string serviceId, string accId)
        {
            return await _dao.GetLastestServiceByProviderAsync(serviceId, accId);
        }

        public async Task UpdateProcessStatusService(string? serviceId)
        {
            await _dao.UpdateProcessStatus(serviceId);
        }
        public async Task<Service> GetByIdOutDelete(string serviceId)
        {
            return await _dao.GetByIdOutDeleteAsync(serviceId);
        }
    }
}
