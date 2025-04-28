using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdatePostRequestDTO
    {
        public string? PostId { get; set; }
        public string? Content { get; set; }
        public string? Privacy { get; set; }

        //List ảnh mới và list ảnh cần xóa
        public bool? IsDeleteAllImage { get; set; }
        public List<IFormFile>? ImagesToAdd { get; set; }
        public List<string>? ImagesToRemove { get; set; }

        //List Hashtag mới và hashtag cần xóa
        public bool? IsDeleteAllHashtag { get; set; }
        public List<string>? HashTagToAdd { get; set; }
        public List<string>? HashTagToRemove { get; set; }

        //List category mới và category cần xóa
        public bool? IsDeleteAllCategory { get; set; }
        public List<string>? CategoriesToAdd { get; set; }
        public List<string>? CategoriesToRemove { get; set; }

        //List Tag friend mới và tag friend cần xóa
        public bool? IsDeleteAllFriend { get; set; }
        public List<string>? PostTagsToAdd { get; set; }
        public List<string>? PostTagsToRemove { get; set; }
    }
}
