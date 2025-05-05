using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CategoryPostService : ICategoryPostService
    {
        private readonly ICategoryPostRepository _categoryRepo;
        public CategoryPostService(ICategoryPostRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<CategoryPostResponseDTO?> Create(Category category)
        {
            if (category == null) return null;
            var create = await _categoryRepo.Create(category);
            if (create == null) return new CategoryPostResponseDTO
            {
                Success = false,
                MessageError = "Cannot create category of post",

            };
            return new CategoryPostResponseDTO
            {
                Success = true,
            };



        }

        public async Task<CategoryPostResponseDTO?> Delete(string categoryId)
        {
            if (categoryId == null) return null;
            var delete = await _categoryRepo.Delete(categoryId);
            if (delete == false)
            {
                return new CategoryPostResponseDTO
                {
                    Success = false,
                    MessageError = "Cannot delete category of post",

                };
            }
            return new CategoryPostResponseDTO
            {
                Success = true,
            };
        }

        public async Task<Category?> GetCategoryById(string? category_id)
        {
            if (string.IsNullOrWhiteSpace(category_id)) return null;
            var category = await _categoryRepo.GetCategoryById(category_id);
            if (category == null) return null;
            return category;

        }

        public async Task<CategoryPostResponseDTO?> GetListCategory()
        {
           
            var list = await _categoryRepo.GetListCategory();
            if (list == null || list.Count == 0) return new CategoryPostResponseDTO
            {
                Success = false,
                Data = null,
                MessageError = "Dont have any category of post!"
            };

            return new CategoryPostResponseDTO
            {
                Success = true,
                Data = list
            };
        }

        public async Task<CategoryPostResponseDTO?> Update(Category category)
        {
            if (category == null) return null;
            var old = await _categoryRepo.GetCategoryById(category.CategoryId);
            if (old == null) return null;
            old.CategoryName = category.CategoryName;
            old.CategoryDescription = category.CategoryDescription;
            old.UpdateAt = DateTime.UtcNow;
            old.AccId = category.AccId;
            var update = await _categoryRepo.Update(old);
            if (update == false)
            {
                return new CategoryPostResponseDTO
                {
                    Success = false,
                    MessageError = "Cannot update category of post",

                };
            }
            return new CategoryPostResponseDTO
            {
                Success = true,
            };
        }
    }
}
