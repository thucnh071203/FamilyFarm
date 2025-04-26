using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<Report>> GetAll();
        Task<Report?> GetById(string id);
        Task<Report?> GetByPostAndReporter(string postId, string reporterId);
        Task<Report> Create(Report report);
        Task<Report> Update(string id, Report report);
        Task Delete(string id);
    }
}
