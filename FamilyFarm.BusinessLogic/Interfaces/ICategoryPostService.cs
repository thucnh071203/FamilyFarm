using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ICategoryPostService
    {
        Task<Category?> GetCategoryById(string? category_id);
        Task<CategoryPostResponseDTO?> GetListCategory();
        Task<CategoryPostResponseDTO?> Delete(string categoryId);
        Task<CategoryPostResponseDTO?> Update(Category category);
        Task<CategoryPostResponseDTO?> Create(Category category);
    }
}
