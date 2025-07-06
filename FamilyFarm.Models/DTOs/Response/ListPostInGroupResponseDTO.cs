using FamilyFarm.Models.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListPostInGroupResponseDTO
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
        public bool? HasMore { get; set; } //Dùng để check list còn hay không, dùng cho chức năng phân trang và infinite scroll
        public int? Count { get; set; }
        public List<PostInGroupMapper>? Data { get; set; }
    }
}
