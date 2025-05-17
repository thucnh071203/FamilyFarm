using FamilyFarm.Models.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class FriendResponseDTO
    {
        public string? Message { get; set; }
        public bool? IsSuccess { get; set; }
        public int? Count { get; set; }
        public List<FriendMapper>? Data { get; set; }
    }
}
