using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class FileUploadRequestDTO
    {
        public IFormFile? imageFile { get; set; }
        public IFormFile? otherFile { get; set; } 
    }
}
