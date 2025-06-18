using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdatePostRequestDTO
    {
        [FromForm]
        public string? PostId { get; set; }
        [FromForm]
        public string? Content { get; set; }
        [FromForm]
        public string? Privacy { get; set; }

        //List ảnh mới và list ảnh cần xóa
        [FromForm]
        public bool? IsDeleteAllImage { get; set; }
        [FromForm]
        public List<IFormFile>? ImagesToAdd { get; set; }
        [FromForm]
        public List<string>? ImagesToRemove { get; set; }

        //List Hashtag mới và hashtag cần xóa
        [FromForm]
        public bool? IsDeleteAllHashtag { get; set; }
        [FromForm]
        public List<string>? HashTagToAdd { get; set; }
        [FromForm]
        public List<string>? HashTagToRemove { get; set; }

        //List category mới và category cần xóa
        [FromForm]
        public bool? IsDeleteAllCategory { get; set; }
        [FromForm]
        public List<string>? CategoriesToAdd { get; set; }
        [FromForm]
        public List<string>? CategoriesToRemove { get; set; }

        //List Tag friend mới và tag friend cần xóa
        [FromForm]
        public bool? IsDeleteAllFriend { get; set; }
        [FromForm]
        public List<string>? PostTagsToAdd { get; set; }
        [FromForm]
        public List<string>? PostTagsToRemove { get; set; }
    }
}
