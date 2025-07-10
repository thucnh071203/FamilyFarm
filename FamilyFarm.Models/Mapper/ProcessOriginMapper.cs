using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class ProcessOriginMapper
    {
        public Process process { get; set; }
        public Service? Service { get; set; }
        public List<ProcessStepMapper>? Steps { get; set; }
    }

    public class ProcessStepMapper
    {
        public ProcessStep Step { get; set; }
        public List<ProcessStepImage>? Images { get; set; }
    }
}
