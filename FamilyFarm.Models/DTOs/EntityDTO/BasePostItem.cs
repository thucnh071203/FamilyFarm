using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class BasePostItem
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } // "Post" hoặc "SharePost"
        public Post? Post { get; set; }
        public SharePost? SharePost { get; set; }
    }
}
