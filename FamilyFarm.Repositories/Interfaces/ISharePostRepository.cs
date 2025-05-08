using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ISharePostRepository
    {
        Task<SharePost?> CreateAsync(SharePost? sharePost);
    }
}
