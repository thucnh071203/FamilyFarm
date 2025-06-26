using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ReportResponseDTO
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public ReportDTO? Data { get; set; }
    }
}
