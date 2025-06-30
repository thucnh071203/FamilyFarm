using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessStepUpdateRequestDTO
    {
        public string? StepId { get; set; }                // null nếu là bước mới
        public int StepNumber { get; set; }
        public string? StepTitle { get; set; }
        public string? StepDescription { get; set; }

        // Danh sách ảnh (có thể chứa ảnh mới và ảnh cũ cần cập nhật)
        public List<StepImageDTO>? ImagesWithId { get; set; }
    }
}
