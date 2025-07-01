using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessUpdateRequestDTO
    {
        // Thông tin của Process
        public string? ServiceId { get; set; }
        public string? ProcessTittle { get; set; }
        public string? Description { get; set; }
        public int NumberOfSteps { get; set; }

        // Danh sách các bước trong Process
        public List<ProcessStepUpdateRequestDTO>? ProcessSteps { get; set; }
        // ✅ thêm mới
        public List<string>? DeletedImageIds { get; set; }
        public List<string>? DeletedStepIds { get; set; }
    }
}
