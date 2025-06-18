using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ReactionDTO
    {
        public Reaction Reaction { get; set; }
        public MyProfileDTO Account { get; set; }
        public CategoryReaction CategoryReaction { get; set; }
    }
}
