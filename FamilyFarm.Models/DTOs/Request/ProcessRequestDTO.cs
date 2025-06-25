using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessRequestDTO
    {
        //PHẦN NÀY LẤY CỦA PROCESS
        public string? ServiceId { get; set; }
        public string? ProcessTittle { get; set; }
        public string? Description { get; set; }
        public int NumberOfSteps { get; set; }

        //PHẦN NÀY LẤY THÔNG TIN PROCESS STEP
        public List<ProcessStepRequestDTO>? ProcessSteps { get; set; }
    }
}
