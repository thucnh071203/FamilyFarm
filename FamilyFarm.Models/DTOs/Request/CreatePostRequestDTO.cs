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
        public List<string>? Hashtags { get; set; } //List string hashtag
        public List<string>? ListCategoryOfPost { get; set; } //List category id
        public List<string>? ListTagFriend { get; set; } //List account id
        public List<IFormFile>? ListImage { get; set; } //List file image
        public string? Privacy {  get; set; }
        public bool? isInGroup { get; set; }
        public string? GroupId { get; set; }
        
    }
}
