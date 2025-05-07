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
        public async Task<List<Process>> GetAllProcessByExpert(string accountId)
        {
            return await _dao.GetAllByExpertAsync(accountId);
        }

        public async Task<List<Process>> GetAllProcessByFarmer(string accountId)
        {
            return await _dao.GetAllByFarmerAsync(accountId);
        }

        public async Task<Process> GetProcessById(string processId)
        {
            return await _dao.GetByIdAsync(processId);
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

        public async Task<List<Process>> GetAllProcessByKeyword(string? keyword, string accountId, string roleId)
        {
            return await _dao.SearchProcessKeywordAsync(keyword, accountId, roleId);
        }

        public async Task<List<Process>> FilterProcessByStatus(string? status, string accountId, string roleId)
        {
            return await _dao.FitlerStatusAsync(status, accountId, roleId);
        }
    }
}
