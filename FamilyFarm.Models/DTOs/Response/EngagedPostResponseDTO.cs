using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class EngagedPostResponseDTO
    {
        public Post Post { get; set; }
        public int TotalReactions { get; set; }
        public int TotalComments { get; set; }
        public int TotalEngagement => TotalReactions + TotalComments;

    }
}
