using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreateReportRequestDTO
    {
        public string PostId { get; set; }
        public string Reason { get; set; }
    }
}
