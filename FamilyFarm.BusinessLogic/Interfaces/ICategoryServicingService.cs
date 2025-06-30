using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ICategoryServicingService
    {
        Task<CategoryServiceResponseDTO> GetAllCategoryService();
        Task<CategoryServiceResponseDTO> GetCategoryServiceById(string categoryServiceId);
        Task<CategoryServiceResponseDTO> CreateCategoryService(CategoryService item);
        Task<CategoryServiceResponseDTO> UpdateCategoryService(string categoryServiceId, CategoryService item);
        Task<CategoryServiceResponseDTO> DeleteCategoryService(string groupRoleId);
        Task<CategoryServiceResponseDTO> GetAllForAdmin();
        Task<CategoryServiceResponseDTO> Restore(string categoryServiceId);
    }
}
