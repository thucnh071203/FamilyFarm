using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class CommentMapper
    {
        public Comment? Comment { get; set; }
        public MyProfileDTO Account { get; set; }
        public List<AccountReactionDTO>? ReactionsOfComment { get; set; }
    }
}
