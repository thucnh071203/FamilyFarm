using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IReportService
    {
        Task<ListReportResponseDTO> GetAll();
        Task<ReportResponseDTO?> GetById(string id);
        Task<Report?> GetByPostAndReporter(string postId, string reporterId);
        Task<ReportResponseDTO> CreateAsync(CreateReportRequestDTO request, string reporterId);
        Task<Report> Update(string id, Report report);
        Task Delete(string id);
    }
}
