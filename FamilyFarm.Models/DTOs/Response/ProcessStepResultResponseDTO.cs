using FamilyFarm.Models.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ProcessStepResultResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ProcessStepResultMapper> Data { get; set; }
    }
}
