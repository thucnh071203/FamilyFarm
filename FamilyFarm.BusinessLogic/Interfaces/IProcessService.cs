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
        Task<ProcessResponseDTO> GetProcessById(string processId);
        Task<ProcessResponseDTO> CreateProcess(ProcessRequestDTO item);
        Task<ProcessResponseDTO> UpdateProcess(string processId, ProcessRequestDTO item);
        Task<ProcessResponseDTO> DeleteProcess(string processId);
        Task<ProcessResponseDTO> GetAllProcessByKeyword(string? keyword);
        //Task<ProcessResponseDTO> FilterProcessByStatus(string? status, string accountId);
    }
}
