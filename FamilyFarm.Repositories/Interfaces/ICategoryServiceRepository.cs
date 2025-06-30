using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ICategoryServiceRepository
    {
        Task<List<CategoryService>> GetAllCategoryService();
        Task<CategoryService> GetCategoryServiceById(string categoryServiceId);
        Task<CategoryService> CreateCategoryService(CategoryService item);
        Task<CategoryService> UpdateCategoryService(string categoryServiceId, CategoryService item);
        Task<long> DeleteCategoryService(string groupRoleId);
        Task<List<CategoryService>> GetAllForAdmin();
        Task<long> Restore(string categoryServiceId);
    }
}
