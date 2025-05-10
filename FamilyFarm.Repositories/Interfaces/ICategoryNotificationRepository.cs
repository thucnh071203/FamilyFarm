using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ICategoryNotificationRepository
    {
        Task<CategoryNotification?> GetByIdAsync(string? id);
        Task<CategoryNotification?> GetByNameAsync(string? name);
    }
}
