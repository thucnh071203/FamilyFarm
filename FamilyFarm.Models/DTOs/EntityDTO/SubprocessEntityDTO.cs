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
        public Service? Service { get; set; }
        public MyProfileDTO? Expert { get; set; }
        public MyProfileDTO? Farmer { get; set; }
        public SubProcess? SubProcess { get; set; }
        public List<ProcessStepEntityDTO>? ProcessSteps { get; set; }
    }
}
