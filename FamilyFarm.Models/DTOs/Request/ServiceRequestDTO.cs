using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class ServiceRequestDTO
    {
        public required string CategoryServiceId { get; set; }
        public required string ProviderId { get; set; }
        public required string ServiceName { get; set; }
        public required string ServiceDescription { get; set; }
        public required decimal Price { get; set; }
        public required int Status { get; set; }
        public required decimal AverageRate { get; set; }
        public required int RateCount { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
