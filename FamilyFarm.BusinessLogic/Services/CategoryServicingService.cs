using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using FamilyFarm.Models.Mapper;
using Microsoft.AspNetCore.SignalR;
using FamilyFarm.BusinessLogic.Hubs;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CategoryServicingService : ICategoryServicingService
    {
        private readonly ICategoryServiceRepository _categoryServiceRepository;
        private readonly IHubContext<CategoryServiceHub> _hubContext;

        public CategoryServicingService(ICategoryServiceRepository categoryServiceRepository, IHubContext<CategoryServiceHub> hub)
        {
            _categoryServiceRepository = categoryServiceRepository;
            _hubContext = hub;
        }

        public async Task<CategoryServiceResponseDTO> GetAllCategoryService()
        {
            var listCategoryService = await _categoryServiceRepository.GetAllCategoryService();

            if (listCategoryService.Count == 0 || listCategoryService == null)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Category list is empty!"
                };
            }

            var mappedList = listCategoryService.Select(c => new ServiceMapper
            {
                categoryService = c
            }).ToList();

            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = mappedList
            };
        }
        public async Task<CategoryServiceResponseDTO> GetAllForAdmin()
        {
            var listCategoryService = await _categoryServiceRepository.GetAllForAdmin();

            if (listCategoryService.Count == 0 || listCategoryService == null)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Category list is empty!"
                };
            }

            var mappedList = listCategoryService.Select(c => new ServiceMapper
            {
                categoryService = c
            }).ToList();

            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = mappedList
            };
        }
        public async Task<CategoryServiceResponseDTO> GetCategoryServiceById(string categoryServiceId)
        {
            var categoryService = await _categoryServiceRepository.GetCategoryServiceById(categoryServiceId);

            if (categoryService == null)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Category not found"
                };
            }

            var mapped = new ServiceMapper
            {
                categoryService = categoryService
            };

            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = new List<ServiceMapper> { mapped }
            };
        }

        public async Task<CategoryServiceResponseDTO> CreateCategoryService(CategoryService item)
        {
            var created = await _categoryServiceRepository.CreateCategoryService(item);

            if(created == null)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to create category"
                };
            }
            // Gửi thông báo SignalR
            await _hubContext.Clients.All.SendAsync("CategoryUpdated");
            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = new List<ServiceMapper>
                {
                    new ServiceMapper { categoryService = created }
                }
            };
        }

        public async Task<CategoryServiceResponseDTO> UpdateCategoryService(string categoryServiceId, CategoryService item)
        {
            var updated = await _categoryServiceRepository.UpdateCategoryService(categoryServiceId, item);
            if (updated == null)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to update category"
                };
            }
            // Gửi thông báo SignalR
            await _hubContext.Clients.All.SendAsync("CategoryUpdated");
            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = new List<ServiceMapper>
                {
                    new ServiceMapper { categoryService = updated }
                }
            };
        }

        public async Task<CategoryServiceResponseDTO> DeleteCategoryService(string categoryServiceId)
        {
            var deletedCount = await _categoryServiceRepository.DeleteCategoryService(categoryServiceId);

            if (deletedCount == 0)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to delete category"
                };
            }
            // Gửi thông báo SignalR
            await _hubContext.Clients.All.SendAsync("CategoryUpdated");
            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = null
            };
        }
        public async Task<CategoryServiceResponseDTO> Restore(string categoryServiceId)
        {
            var deletedCount = await _categoryServiceRepository.Restore(categoryServiceId);

            if (deletedCount == 0)
            {
                return new CategoryServiceResponseDTO
                {
                    Success = false,
                    Message = "Failed to restore category"
                };
            }
            // Gửi thông báo SignalR
            await _hubContext.Clients.All.SendAsync("CategoryUpdated");
            return new CategoryServiceResponseDTO
            {
                Success = true,
                Message = null,
                Data = null
            };
        }
    }
}
