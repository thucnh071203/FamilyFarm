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
    public interface IProcessService
    {
        Task<ProcessResponseDTO> GetAllProcess();
        //Task<ProcessResponseDTO> GetAllProcessByExpert(string accountId);
        //Task<ProcessResponseDTO> GetAllProcessByFarmer(string accountId);
        Task<ProcessOriginResponseDTO> GetProcessById(string serviceId);
        Task<ProcessOriginResponseDTO> GetProcessByProcessId(string processId);
        Task<ProcessResponseDTO> CreateProcess(ProcessRequestDTO item);
        Task<ProcessResponseDTO> UpdateProcess(string processId, ProcessUpdateRequestDTO item);
        Task<ProcessResponseDTO> DeleteProcess(string processId);
        Task<ProcessResponseDTO> GetAllProcessByKeyword(string? keyword);
        //Task<ProcessResponseDTO> FilterProcessByStatus(string? status, string accountId);

        //SUBPROCESS
        Task<bool?> CreateSubprocess(string? expertId, CreateSubprocessRequestDTO request);
        Task<ListSubprocessResponseDTO?> GetListSubprocessByExpert(string? expertId);
        Task<ListSubprocessResponseDTO?> GetListSubprocessByFarmer(string? farmerId);
        Task<SubProcessResponseDTO?> GetListSubProcessCompleted();
        Task<ProcessStepResultResponseDTO> CreateProcessStepResult(ProcessStepResultRequestDTO request);
        Task<ProcessStepResultResponseDTO> GetProcessStepResultsByStepId(string stepId);
        Task<bool?> IsCompletedSubprocess(string? subprocessId);
        Task<bool?> ConfirmSubprocess(string? subprocessId, string? bookingServiceId);
    }
}
