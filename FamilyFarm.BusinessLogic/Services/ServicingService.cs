using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ServicingService : IServicingService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ICategoryServiceRepository _categoryServiceRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUploadFileService _uploadFileService;
        private readonly IProcessRepository _processRepository;
        private readonly IProcessStepRepository _processStepRepository;
        private readonly IMapper _mapper;

        public ServicingService(IServiceRepository serviceRepository, ICategoryServiceRepository categoryServiceRepository, IAccountRepository accountRepository, IUploadFileService uploadFileService, IProcessRepository processRepository, IProcessStepRepository processStepRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _categoryServiceRepository = categoryServiceRepository;
            _accountRepository = accountRepository;
            _uploadFileService = uploadFileService;
            _processRepository = processRepository;
            _processStepRepository = processStepRepository;
            _mapper = mapper;
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

            //KIỂM TRA LẠI SERVICE NÀO KHÔNG CÓ PROCESS THÌ UPDATE STATUS LẠI
            foreach (var service in listAllService)
            {
                //var processOfService = await _processRepository.GetProcessByServiceId(service.ServiceId);
                if (service.HaveProcess != false)
                {
                    continue;
                }
                await _serviceRepository.UpdateStatusService(service.ServiceId, 0);//NẾU KHÔNG CÓ PROCESS THÌ UPDATE LẠI UNAVAILABLE
            }

            var servicesAfterUpdate = await _serviceRepository.GetAllService();

            /* var serviceMappers = new List<ServiceMapper>();

             foreach (var service in listAllService)
             {
                 serviceMappers.Add(new ServiceMapper
                 {
                     service = service
                 });
             }*/

            // Viết tắt

            var serviceMappers = servicesAfterUpdate.Select(s => new ServiceMapper { service = s }).ToList();

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

            //LẤY DANH SÁCH SERVICE CỦA NGƯỜI DÙNG ĐÓ
            var services = await _serviceRepository.GetAllServiceByProvider(providerId);

            if (services.Count == 0 || services == null)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Service list is empty"
                };
            }

            //KIỂM TRA LẠI SERVICE NÀO KHÔNG CÓ PROCESS THÌ UPDATE STATUS LẠI
            foreach(var service in services)
            {
                //var processOfService = await _processRepository.GetProcessByServiceId(service.ServiceId);
                if(service.HaveProcess == true)
                {
                    continue;
                }
                await _serviceRepository.UpdateStatusService(service.ServiceId, 0);//NẾU KHÔNG CÓ PROCESS THÌ UPDATE LẠI UNAVAILABLE
            }

            var servicesAfterUpdate = await _serviceRepository.GetAllServiceByProvider(providerId);

            var serviceMappers = servicesAfterUpdate.Select(s => new ServiceMapper { service = s }).ToList();

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

            // Load provider profile
            var account = await _accountRepository.GetAccountByAccId(service.ProviderId);
            var provider = _mapper.Map<MyProfileDTO>(account);

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
                Data = new List<ServiceMapper> { new ServiceMapper { service = service, Provider = provider } }
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

            if (item.Price <= 0)
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Price must greater than 0"
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
            if (string.IsNullOrEmpty(serviceId))
            {
                return new ServiceResponseDTO
                {
                    Success = false,
                    Message = "Service not found"
                };
            }

            var process = await _processRepository.GetProcessByServiceId(serviceId);

            if (process != null)
            {
                //XOA CAC BANG LIEN QUAN
                var processSteps = await _processStepRepository.GetStepsByProcessId(process.ProcessId);
                if(processSteps != null && processSteps.Count > 0)
                {
                    foreach (var step in processSteps)
                    {
                        //XOA IMAGE CUA STEP
                        await _processStepRepository.DeleteImagesByStepId(step.StepId);
                        //XOA STEP DO
                        await _processStepRepository.DeleteStepById(step.StepId);
                    }
                }
                
                //XOA PROCESS CUA SERVICE DO
                var isDeleteProcess = await _processRepository.HardDeleteByService(serviceId);
            }

            
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

        public async Task<ServiceDetailResponseDTO> GetServiceDetail(string serviceId)
        {
            var service = await _serviceRepository.GetServiceById(serviceId);

            if (service == null)
            {
                return new ServiceDetailResponseDTO
                {
                    Success = false,
                    Message = "Service not found"
                };
            }

            var expert = await _accountRepository.GetAccountByAccId(service.ProviderId);

            if (expert == null)
            {
                return new ServiceDetailResponseDTO
                {
                    Success = false,
                    Message = "Expert not found"
                };
            }

            var category = await _categoryServiceRepository.GetCategoryServiceById(service.CategoryServiceId);
            if (category == null)
            {
                return new ServiceDetailResponseDTO
                {
                    Success = false,
                    Message = "Category not found"
                };
            }

            var serviceDetail = new ServiceDetailMapper
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                ServiceDescription = service.ServiceDescription,
                Price = service.Price,
                ImageUrl = service.ImageUrl,
                Status = service.Status,
                AverageRate = service.AverageRate,
                RateCount = service.RateCount,
                CreateAt = service.CreateAt,
                UpdateAt = service.UpdateAt,
                IsDeleted = service.IsDeleted,
                HaveProcess = service.HaveProcess,
                RoleId = expert?.RoleId,
                FullName = expert?.FullName,
                Avatar = expert?.Avatar,
                CategoryName = category?.CategoryName
            };

            return new ServiceDetailResponseDTO
            {
                Success = true,
                Message = "Get service successfully",
                Data = serviceDetail
            };
        }
    }
}
