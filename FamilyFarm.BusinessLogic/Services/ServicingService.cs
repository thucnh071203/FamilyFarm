using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ServicingService : IServicingService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServicingService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<ServiceResponseDTO> GetAllService()
        {
            var listAllService = await _serviceRepository.GetAllService();

            if (listAllService.Count == 0 || listAllService == null) {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Service list is empty"
                };
            }

            /* var serviceMappers = new List<ServiceMapper>();

             foreach (var service in listAllService)
             {
                 serviceMappers.Add(new ServiceMapper
                 {
                     service = service
                 });
             }*/

            // Viết tắt

            var serviceMappers = listAllService.Select(s => new ServiceMapper { service = s }).ToList();

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Get all services successfully",
                Data = serviceMappers
            };
        }

        public async Task<ServiceResponseDTO> GetAllServiceByProvider(string providerId)
        {
            var services = await _serviceRepository.GetAllServiceByProvider(providerId);

            if (services.Count == 0 || services == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Service list is empty"
                };
            }

            var serviceMappers = services.Select(s => new ServiceMapper { service = s }).ToList();

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Get all services successfully",
                Data = serviceMappers
            };
        }

        public async Task<ServiceResponseDTO> GetServiceById(string serviceId)
        {
            var service = await _serviceRepository.GetServiceById(serviceId);

            if (service == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Service not found"
                };
            }

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Get service successfully",
                Data = new List<ServiceMapper> { new ServiceMapper { service = service } }
            };
        }

        public async Task<ServiceResponseDTO> CreateService(Service item)
        {
            var created = await _serviceRepository.CreateService(item);

            if (created == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to create service"
                };
            }

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Service created successfully",
                Data = new List<ServiceMapper> { new ServiceMapper { service = created } }
            };
        }

        public async Task<ServiceResponseDTO> UpdateService(string serviceId, Service item)
        {
            var updated = await _serviceRepository.UpdateService(serviceId, item);

            if (updated == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to update service"
                };
            }

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Service updated successfully",
                Data = new List<ServiceMapper> { new ServiceMapper { service = updated } }
            };
        }

        public async Task<ServiceResponseDTO> DeleteService(string serviceId)
        {
            var deletedCount = await _serviceRepository.DeleteService(serviceId);

            if (deletedCount == 0)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to delete service"
                };
            }

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Service deleted successfully",
                Data = null
            };
        }
    }
}
