using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class SubprocessEntityDTO
    {
        public SubProcess? SubProcess { get; set; }
        public List<ProcessStepEntityDTO>? ProcessSteps { get; set; }
    }
}
