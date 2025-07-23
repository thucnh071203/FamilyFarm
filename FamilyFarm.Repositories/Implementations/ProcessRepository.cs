using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly ProcessDAO _dao;
        public ProcessRepository(ProcessDAO dao)
        {
            _dao = dao;
        }
        public async Task<List<Process>> GetAllProcess()
        {
            return await _dao.GetAllAsync();
        }
        //public async Task<List<Process>> GetAllProcessByExpert(string accountId)
        //{
        //    return await _dao.GetAllByExpertAsync(accountId);
        //}

        //public async Task<List<Process>> GetAllProcessByFarmer(string accountId)
        //{
        //    return await _dao.GetAllByFarmerAsync(accountId);
        //}
        public async Task<Process> GetProcessByProcessId(string processId)
        {
            return await _dao.GetByIdAsync(processId);
        }

        public async Task<Process> GetProcessById(string serviceId)
        {
            return await _dao.GetLatestByServiceIdAsync(serviceId);
        }

        public async Task<Process> CreateProcess(Process item)
        {
            return await _dao.CreateAsync(item);
        }

        public async Task<Process> UpdateProcess(string processId, Process item)
        {
            return await _dao.UpdateAsync(processId, item);
        }

        public async Task<long> DeleteProcess(string processId)
        {
            return await _dao.DeleteAsync(processId);
        }

        public async Task<List<Process>> GetAllProcessByKeyword(string? keyword)
        {
            return await _dao.SearchProcessKeywordAsync(keyword);
        }

        public async Task<Process?> GetProcessByServiceId(string? serviceId)
        {
            return await _dao.GetProcessByServiceId(serviceId);
        }

        //public async Task<List<Process>> FilterProcessByStatus(string? status, string accountId, string roleId)
        //{
        //    return await _dao.FitlerStatusAsync(status, accountId, roleId);
        //}

        public async Task<bool?> HardDeleteByService(string? serviceId)
        {
            return await _dao.HardDeleteByServiceId(serviceId);
        }

        public async Task<SubProcess?> CreateSubprocess(SubProcess? request)
        {
            return await _dao.CreateSubprocess(request);
        }

        public async Task<List<SubProcess>?> GetAllSubprocessByExpert(string? expertId)
        {
            return await _dao.GetSubprocessesByExpert(expertId);
        }

        public async Task<List<SubProcess>?> GetAllSubprocessByFarmer(string? farmerId)
        {
            return await _dao.GetSubprocessesByFarmer(farmerId);
        }

        public async Task<SubProcess?> GetSubProcessBySubProcessId(string? subProcessId)
        {
            return await _dao.GetBySubProcessId(subProcessId);
        }

        public async Task<SubProcess?> UpdateSubProcess(string? subProcessId, SubProcess subProcess)
        {
            return await _dao.UpdateSubProcess(subProcessId, subProcess);
        }

        public async Task<List<SubProcess>?> GetAllSubProcessCompleted()
        {
            return await _dao.GetAllSubProcessComplete();
        }

        public async Task<bool?> UpdateContinueStep(string? subprocessId, int stepNumber)
        {
            return await _dao.UpdateContinueStep(subprocessId, stepNumber);
        }

        public async Task UpdateStatusSubprocess(string? subprocessId, string? status)
        {
            await _dao.UpdateStatusSubprocess(subprocessId, status);
        }
    }
}
