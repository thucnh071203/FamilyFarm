using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CreatePostRequestDTO
    {
        public string? PostContent { get; set; }
        public List<string>? Hashtags { get; set; }
        public List<string>? ListCategoryOfPost { get; set; }
        public List<string>? ListTagFriend { get; set; }
        public List<IFormFile>? ListImage { get; set; }
        public string? Privacy {  get; set; }

    }
}
