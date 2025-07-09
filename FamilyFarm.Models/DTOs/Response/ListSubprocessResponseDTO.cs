using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListSubprocessResponseDTO
    {
       public string? Message { get; set; }
       public bool? Success { get; set; }
       public int Count { get; set; }
       public List<SubprocessEntityDTO>? Subprocesses { get; set; }
    }
}
