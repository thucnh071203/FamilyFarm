using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreateSubprocessRequestDTO
    {
        public string? FarmerId { get; set; }
        public string? BookingServiceId { get; set; }
        public string? ProcessId { get; set; }
        public string? Description  { get; set; }
        public string? Title { get; set; }
        public int NumberOfSteps { get; set; }
        public bool? IsExtraProcess { get; set; }

        //PHẦN NÀY LẤY THÔNG TIN PROCESS STEP
        public List<ProcessStepRequestDTO>? ProcessSteps { get; set; }
    }
}
