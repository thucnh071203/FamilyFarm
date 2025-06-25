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
        Task<ProcessStep?> CreateProcessStep(ProcessStep? processStep);

        //Process Step image
        Task CreateStepImage(ProcessStepImage? request);
    }
}
