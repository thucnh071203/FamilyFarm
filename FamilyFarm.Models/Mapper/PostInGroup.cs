using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class PostInGroup
    {
        public Post post { get; set; }
        public MiniAccountDTO account { get; set; }
    }
}
