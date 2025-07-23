using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessStepResultRequestDTO
    {
        public string? SubprocessId { get; set; }
        public string StepId { get; set; }
        public string? StepResultComment { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
