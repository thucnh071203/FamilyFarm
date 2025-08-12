using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessRequestDTO
    {
        //PHẦN NÀY LẤY CỦA PROCESS
        public string? ServiceId { get; set; }
        public string? ProcessTittle { get; set; }
        public string? Description { get; set; }
        public int NumberOfSteps { get; set; }
        public int Status { get; set; } // 0/1
        //PHẦN NÀY LẤY THÔNG TIN PROCESS STEP
        public List<ProcessStepRequestDTO>? ProcessSteps { get; set; }
    }

    //public class ProcessStepFormDTO
    //{
    //    public int StepNumber { get; set; }
    //    public string? StepTitle { get; set; }
    //    public string? StepDescription { get; set; }
    //    public List<IFormFile>? Images { get; set; }
    //}
}
