using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UpdateProfileResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string? MessageError { get; set; }
        public int Count { get; set; }
        public Account Data { get; set; }
    }
}
