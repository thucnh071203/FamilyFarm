using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class SendMessageRequestDTO
    {
        public string? Message { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public IFormFile? File { get; set; }
        public required string ReceiverId { get; set; }
    }
}
