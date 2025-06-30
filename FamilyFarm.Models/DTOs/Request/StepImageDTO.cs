using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class StepImageDTO
    {
        public string? ProcessStepImageId { get; set; }    // null nếu là ảnh mới
        public string? ImageUrl { get; set; }
    }
}
