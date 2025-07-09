using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ProcessStepEntityDTO
    {
        public ProcessStep? ProcessStep { get; set; }
        public List<ProcessStepImage>? ProcessStepImages { get; set; }
    }
}
