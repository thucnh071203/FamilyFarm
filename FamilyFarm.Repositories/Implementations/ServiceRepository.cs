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
    public class ServiceRepository:IServiceRepository
    {
        private readonly ServiceDAO serviceDAO;
        public ServiceRepository(ServiceDAO serviceDAO)
        {
            this.serviceDAO = serviceDAO;
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await serviceDAO.GetAllAsync();
        }

        public async Task<List<Service>> GetAllByProviderIdAsync(string providerId)
        {
            return await serviceDAO.GetAllByProviderIdAsync(providerId);
        }

        public async Task<Service?> GetByIdAsync(string serviceId)
        {
           return await serviceDAO.GetByIdAsync(serviceId); ;
        }
    }
}
