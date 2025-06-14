﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ServicingService : IServicingService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ICategoryServiceRepository _categoryServiceRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUploadFileService _uploadFileService;

        public ServicingService(IServiceRepository serviceRepository, ICategoryServiceRepository categoryServiceRepository, IAccountRepository accountRepository, IUploadFileService uploadFileService)
        {
            _serviceRepository = serviceRepository;
            _categoryServiceRepository = categoryServiceRepository;
            _accountRepository = accountRepository;
            _uploadFileService = uploadFileService;
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
                Count = listAllService.Count,
                Data = serviceMappers
            };
        }

        public async Task<ServiceResponseDTO> GetAllServiceByProvider(string providerId)
        {
            var checkAccount = await _accountRepository.GetAccountById(providerId);

            if (checkAccount == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is null"
                };
            }
            else if (checkAccount.RoleId != "68007b2a87b41211f0af1d57")
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Account is not expert"
                };
            }

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
                Count = services.Count,
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

        public async Task<ServiceResponseDTO> CreateService(ServiceRequestDTO item)
        {
            if (item == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var checkCategory = await _categoryServiceRepository.GetCategoryServiceById(item.CategoryServiceId);

            if (checkCategory == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Category is null"
                };
            }

            var imageURL = await _uploadFileService.UploadImage(item.ImageUrl);

            var addNewService = new Service 
            {
                ServiceId = null,
                CategoryServiceId = item.CategoryServiceId,
                ProviderId = item.ProviderId,
                ServiceName = item.ServiceName,
                ServiceDescription = item.ServiceDescription,
                Price = item.Price,
                ImageUrl = imageURL.UrlFile ?? "",
                Status = item.Status,
                AverageRate = item.AverageRate,
                RateCount = item.RateCount
            };
            
            var created = await _serviceRepository.CreateService(addNewService);

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

        public async Task<ServiceResponseDTO> UpdateService(string serviceId, ServiceRequestDTO item)
        {
            if (item == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var checkCategory = await _categoryServiceRepository.GetCategoryServiceById(item.CategoryServiceId);

            if (checkCategory == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Category is null"
                };
            }

            var checkOwner = await _serviceRepository.GetServiceById(serviceId);

            if (checkOwner.ProviderId != item.ProviderId)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Provider does not match"
                };
            }

            //if (item.ImageUrl == null) item.ImageUrl = "default.jpg";

            //var imageURL = await _uploadFileService.UploadImage(item.ImageUrl);

            string finalImageUrl = checkOwner.ImageUrl; // mặc định giữ ảnh cũ

            if (item.ImageUrl != null)
            {
                var imageURL = await _uploadFileService.UploadImage(item.ImageUrl);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalImageUrl = imageURL.UrlFile;
                }
            }

            var updateService = new Service
            {
                ServiceId = null,
                CategoryServiceId = item.CategoryServiceId,
                ProviderId = item.ProviderId,
                ServiceName = item.ServiceName,
                ServiceDescription = item.ServiceDescription,
                Price = item.Price,
                ImageUrl = finalImageUrl,
                Status = item.Status,
                AverageRate = item.AverageRate,
                RateCount = item.RateCount
            };

            var updated = await _serviceRepository.UpdateService(serviceId, updateService);

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

        public async Task<ServiceResponseDTO> ChangeStatusService(string serviceId)
        {
            var changeStatus = await _serviceRepository.ChangeStatusService(serviceId);

            if (changeStatus == 0)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to change status service"
                };
            }

            return new ServiceResponseDTO
            {
                Success = true,
                Message = "Service change status successfully",
                Data = null
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
