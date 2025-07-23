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
        Task<ProcessStep?> GetStepById(string? stepId);
        Task<ProcessStep?> CreateProcessStep(ProcessStep? processStep);
        Task<ProcessStep?> UpdateProcessStep(string stepId, ProcessStep? processStep);
        Task DeleteStepById(string stepId);

        //Process Step image
        Task<List<ProcessStepImage>> GetStepImagesByStepId(string stepId);
        Task CreateStepImage(ProcessStepImage? request);
        Task<ProcessStepImage?> UpdateStepImage(string imageId, ProcessStepImage? request);
        Task DeleteStepImageById(string imageId);
        Task DeleteImagesByStepId(string stepId);

        //SUBPROCESS
        Task<List<ProcessStep>?> GetStepsBySubprocess(string? subprocessId);
        Task<ProcessStepResults> CreateProcessStepResult(ProcessStepResults result);
        Task<List<ProcessStepResults>> GetProcessStepResultsByStepId(string stepId);
        Task<StepResultImages> CreateStepResultImage(StepResultImages image);
        Task<List<StepResultImages>> GetStepResultImagesByStepResultId(string stepResultId);
        
    }
}
