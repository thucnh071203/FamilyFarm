using FamilyFarm.Models.DTOs.EntityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class GroupCardResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Count { get; set; }
        public List<GroupCardDTO>? Data { get; set; }
    }
}
