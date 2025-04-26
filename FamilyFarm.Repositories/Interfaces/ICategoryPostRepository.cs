using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ICategoryPostRepository
    {
        Task<Category?> GetCategoryById(string? category_id);
    }
}
