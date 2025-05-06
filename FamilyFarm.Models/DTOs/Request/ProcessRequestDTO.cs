using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ProcessRequestDTO
    {
        //public required string ExpertId { get; set; }
        public string? ExpertId { get; set; }
        public required string FarmerId { get; set; }
        public required string ProcessTittle { get; set; }
        public required string Description { get; set; }
        public required int NumberOfSteps { get; set; }
        public required int ContinueStep { get; set; }
        public required string ProcessStatus { get; set; }
        public bool? IsCompletedByExpert { get; set; }
        public bool? IsCompletedByFarmer { get; set; }
    }
}
