using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class SharePostRequestDTO
    {
        public required string PostId { get; set; }
        public string? SharePostContent { get; set; }
        public List<string>? HashTags { get; set; }
        public List<string>? TagFiendIds { get; set; }
        public string? SharePostScope { get; set; } // Public, Private, Draft,...
    }
}
