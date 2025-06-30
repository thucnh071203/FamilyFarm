using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IProcessStepRepository
    {
        Task<List<ProcessStep>> GetStepsByProcessId(string processId);
        Task<ProcessStep?> CreateProcessStep(ProcessStep? processStep);
        Task<ProcessStep?> UpdateProcessStep(string stepId, ProcessStep? processStep);

        //Process Step image
        Task<List<ProcessStepImage>> GetStepImagesByStepId(string stepId);
        Task CreateStepImage(ProcessStepImage? request);
        Task<ProcessStepImage?> UpdateStepImage(string imageId, ProcessStepImage? request);
    }
}
