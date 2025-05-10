using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class SavedPostMapper
    {
        public Post? Post { get; set; }
        public DateTime? SavedAt { get; set; }
        public MyProfileDTO? Account { get; set; }
    }
}
