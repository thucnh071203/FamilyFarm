using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class SharePostMapper
    {
        public int? ReactionCount { get; set; } = 0;
        public int? CommentCount { get; set; } = 0;
        public int? ShareCount { get; set; } = 0;
        public List<HashTag>? HashTags { get; set; }
        public List<SharePostTag>? SharePostTags { get; set; }
        public SharePost? SharePost { get; set; }
        public MyProfileDTO? OwnerSharePost { get; set; }
        public PostMapper? OriginalPost { get; set; } // Post gốc được share
    }
}
