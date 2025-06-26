using FamilyFarm.Models.DTOs.EntityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListReportResponseDTO
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
        public List<ReportDTO> Data { get; set; }
    }
}
