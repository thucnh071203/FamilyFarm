using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
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
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository reportRepository, IAccountRepository accountRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _accountRepository = accountRepository;
            _mapper = mapper;
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

        public async Task<ReportResponseDTO> CreateAsync(CreateReportRequestDTO request, string reporterId)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(reporterId) || string.IsNullOrEmpty(request.PostId) || string.IsNullOrEmpty(request.Reason))
                {
                    return new ReportResponseDTO
                    {
                        Success = false,
                        Message = "Invalid input data.",
                        Report = null
                    };
                }

                // Tạo model Report
                var report = new Report
                {
                    ReportId = Guid.NewGuid().ToString(), // Hoặc để MongoDB tự sinh ObjectId
                    ReporterId = reporterId,
                    PostId = request.PostId,
                    Reason = request.Reason,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                // Lưu báo cáo vào cơ sở dữ liệu
                var createdReport = await _reportRepository.Create(report);
                if (createdReport == null)
                {
                    return new ReportResponseDTO
                    {
                        Success = false,
                        Message = "Can not report.",
                        Report = null
                    };
                }

                // Lấy thông tin tài khoản của người báo cáo
                var reporter = await _accountRepository.GetAccountById(reporterId);
                if (reporter == null)
                {
                    return new ReportResponseDTO
                    {
                        Success = false,
                        Message = "Reporter information not found.",
                        Report = null
                    };
                }

                // Ánh xạ Account sang MiniAccountDTO
                var miniAccountDTO = _mapper.Map<MiniAccountDTO>(reporter);

                // Tạo ReportDTO
                var reportDTO = new ReportDTO
                {
                    Report = createdReport,
                    Reporter = miniAccountDTO
                };

                // Trả về phản hồi
                return new ReportResponseDTO
                {
                    Success = true,
                    Message = "The report was created successfully.",
                    Report = reportDTO
                };
            }
            catch (Exception ex)
            {
                return new ReportResponseDTO
                {
                    Success = false,
                    Message = $"Error generating report: {ex.Message}",
                    Report = null
                };
            }
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
