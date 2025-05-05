using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;

namespace FamilyFarm.Models.DTOs.Response
{
    public class MyProfileResponseDTO
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
        public MyProfileDTO? Data { get; set; }
    }
}
