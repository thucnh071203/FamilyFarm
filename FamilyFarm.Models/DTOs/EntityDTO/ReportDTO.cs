using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ReportDTO
    {
        public Report Report { get; set; }
        public MiniAccountDTO Reporter { get; set; }
        public PostMapper? Post { get; set; }
    }
}
