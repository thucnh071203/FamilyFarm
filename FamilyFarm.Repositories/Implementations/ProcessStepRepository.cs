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
    public class ProcessStepRepository : IProcessStepRepository
    {
        private readonly ProcessStepDAO _stepDao;
        private readonly ProcessStepImageDAO _stepImageDao;
        public ProcessStepRepository(ProcessStepDAO stepDao, ProcessStepImageDAO stepImageDao)
        {
            _stepDao = stepDao;
            _stepImageDao = stepImageDao;
        }
        public async Task<List<ProcessStep>> GetStepsByProcessId(string processId)
        {
            return await _stepDao.GetStepsByProcessId(processId);
        }
        public async Task<ProcessStep?> CreateProcessStep(ProcessStep? processStep)
        {
            return await _stepDao.CreateStep(processStep);
        }
        public async Task<ProcessStep?> UpdateProcessStep(string stepId, ProcessStep? processStep)
        {
            return await _stepDao.EditStep(stepId, processStep);
        }
        public async Task<List<ProcessStepImage>> GetStepImagesByStepId(string stepId)
        {
            return await _stepImageDao.GetStepImagesByStepId(stepId);
        }
        public async Task CreateStepImage(ProcessStepImage? request)
        {
            await _stepImageDao.CreateStepImage(request);
        }

        public async Task<ProcessStepImage?> UpdateStepImage(string imageId, ProcessStepImage? request)
        {
            return await _stepImageDao.UpdateStepImage(imageId, request);
        }
    }
}
