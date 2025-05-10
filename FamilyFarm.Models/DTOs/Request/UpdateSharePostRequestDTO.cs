using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdateSharePostRequestDTO
    {
        public string? SharePostContent { get; set; }
        public string? SharePostScope { get; set; }
        [DefaultValue(false)]
        public bool? IsDeleteAllHashtag { get; set; }
        public List<string>? HashTagToAdd { get; set; }
        public List<string>? HashTagToRemove { get; set; }
        [DefaultValue(false)] 
        public bool? IsDeleteAllFriend { get; set; }
        public List<string>? TagFiendIdsToAdd { get; set; }
        public List<string>? TagFiendIdsToRemove { get; set; }
    }
}
