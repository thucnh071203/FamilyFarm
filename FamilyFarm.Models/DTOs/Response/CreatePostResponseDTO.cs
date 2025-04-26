using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class CreatePostResponseDTO
    {
        public string? Message { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
