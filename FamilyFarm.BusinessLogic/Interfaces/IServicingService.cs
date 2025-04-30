using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IServicingService
    {
        Task<ServiceResponseDTO> GetAllService();
        Task<ServiceResponseDTO> GetAllServiceByProvider(string providerId);
        Task<ServiceResponseDTO> GetServiceById(string serviceId);
        Task<ServiceResponseDTO> CreateService(Service service);
        Task<ServiceResponseDTO> UpdateService(string serviceId, Service service);
        Task<ServiceResponseDTO> DeleteService(string serviceId);
    }
}
