using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessStepRequestDTO
    {
        public int StepNumber { get; set; }
        public string? StepTitle { get; set; }
        public string? StepDescription { get; set; }
        public List<string>? Images { get; set; }
    }
}
