using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<Report>> GetAll()
        {
            return await _reportRepository.GetAll();
        }

        public async Task<Report?> GetById(string id)
        {
            return await _reportRepository.GetById(id);
        }

        public async Task<Report?> GetByPostAndReporter(string postId, string reporterId)
        {
            return await _reportRepository.GetByPostAndReporter(postId, reporterId);
        }

        public async Task<Report> Create(Report report)
        {
            return await _reportRepository.Create(report);
        }

        public async Task<Report> Update(string id, Report report)
        {
            return await _reportRepository.Update(id, report);
        }

        public async Task Delete(string id)
        {
            await _reportRepository.Delete(id);
        }

    }
}
