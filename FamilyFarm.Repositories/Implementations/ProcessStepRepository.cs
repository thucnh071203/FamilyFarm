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

        public async Task<ProcessStep?> CreateProcessStep(ProcessStep? processStep)
        {
            return await _stepDao.CreateStep(processStep);
        }

        public async Task CreateStepImage(ProcessStepImage? request)
        {
            await _stepImageDao.CreateStepImage(request);
        }
    }
}
