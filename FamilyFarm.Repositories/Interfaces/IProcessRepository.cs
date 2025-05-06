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
        Task<List<Process>> GetAllProcessByExpert(string accountId);
        Task<List<Process>> GetAllProcessByFarmer(string accountId);
        Task<Process> GetProcessById(string processId);
        Task<Process> CreateProcess(Process item);
        Task<Process> UpdateProcess(string processId, Process item);
        Task<long> DeleteProcess(string processId);
    }
}
