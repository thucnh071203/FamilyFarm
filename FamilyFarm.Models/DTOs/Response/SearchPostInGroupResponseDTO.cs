using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class SearchPostInGroupResponseDTO
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
        public List<PostInGroup>? Posts { get; set; }
    }
}
