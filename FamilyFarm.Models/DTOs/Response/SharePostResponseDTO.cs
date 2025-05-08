using FamilyFarm.Models.DTOs.EntityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class SharePostResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SharePostDTO? Data { get; set; }
    }
}
