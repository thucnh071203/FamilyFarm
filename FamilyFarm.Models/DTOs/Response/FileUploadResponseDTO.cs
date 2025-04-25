using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class FileUploadResponseDTO
    {
        public string? Message { get; set; }
        public string? UrlFile { get; set; }
        public string? TypeFile { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
