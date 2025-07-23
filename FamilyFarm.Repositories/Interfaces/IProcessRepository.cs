using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IProcessRepository
    {
        Task<List<Process>> GetAllProcess();
        //Task<List<Process>> GetAllProcessByExpert(string accountId);
        //Task<List<Process>> GetAllProcessByFarmer(string accountId);
        Task<Process> GetProcessByProcessId(string processId);
        Task<Process> GetProcessById(string serviceId);
        Task<Process> CreateProcess(Process item);
        Task<Process> UpdateProcess(string processId, Process item);
        Task<long> DeleteProcess(string processId);
        Task<List<Process>> GetAllProcessByKeyword(string? keyword);
        //Task<List<Process>> FilterProcessByStatus(string? status, string accountId, string roleId);
        Task<Process?> GetProcessByServiceId(string? serviceId);
        Task<bool?> HardDeleteByService(string? serviceId);

        //SUBPROCESS
        Task<List<SubProcess>?> GetAllSubprocessByExpert(string? expertId);
        Task<List<SubProcess>?> GetAllSubprocessByFarmer(string? farmerId);
        Task<SubProcess?> CreateSubprocess(SubProcess? request);
        Task<SubProcess?> GetSubProcessBySubProcessId(string? subProcessId);
        Task<SubProcess?> UpdateSubProcess(string? subProcessId, SubProcess subProcess);
        Task<List<SubProcess>?> GetAllSubProcessCompleted();
        Task UpdateStatusSubprocess(string? subprocessId, string? status);
        Task<bool?> UpdateContinueStep(string? subprocessId, int stepNumber);
    }
}
