using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDAO _reportDAO;

        public ReportRepository(ReportDAO reportDAO)
        {
            _reportDAO = reportDAO;
        }

        public async Task<List<Report>> GetAll()
        {
            return await _reportDAO.GetAllAsync();
        }

        public async Task<Report?> GetById(string id)
        {
             return await _reportDAO.GetByIdAsync(id);
        }
        public async Task<Report?> GetByPostAndReporter(string postId, string reporterId)
        {
            return await _reportDAO.GetByPostAndReporterAsync(postId, reporterId);
        }

        public async Task<Report> Create(Report report)
        {
             return await _reportDAO.CreateAsync(report);
        }

        public async Task<Report> Update(string id, Report report)
        {
             return await _reportDAO.UpdateAsync(id, report);
        }

        public async Task Delete(string id)
        {
             await _reportDAO.DeleteAsync(id);
        }
    }
}
