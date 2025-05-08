using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class SharePostDTO
    {
        public SharePost? SharePost { get; set; }
        public List<HashTag>? HashTags { get; set; }
        public List<SharePostTag>? SharePostTags { get; set; }
        public PostMapper? PostData { get; set; }
    }
}
