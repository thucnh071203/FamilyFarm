using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.Response
{
    public class GroupResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Count { get; set; }
        public List<Group>? Data { get; set; }
    }
}
