using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class ProcessStepResultMapper
    {
        public ProcessStepResults Result { get; set; }
        public List<StepResultImages>? Images { get; set; }
    }
}
