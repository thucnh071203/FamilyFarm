using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class PostMapper
    {
        public Post? Post { get; set; }
        public MyProfileDTO? OwnerPost { get; set; }
        public List<PostCategory>? PostCategories { get; set; }
        public List<PostImage>? PostImages { get; set; }
        public List<HashTag>? HashTags { get; set; }
        public List<PostTag>? PostTags { get; set; }
    }
}
