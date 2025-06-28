using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FamilyFarm.Models.Mapper
{
    public class ServiceDetailMapper
    {
        // Trả về service
        public string? ServiceId { get; set; }
        public string? CategoryServiceId { get; set; }
        public string? ProviderId { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; }
        public decimal? AverageRate { get; set; }
        public int? RateCount { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? HaveProcess { get; set; }
        // Trả về Expert
        public string? RoleId { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        // Trả về service category
        public string? CategoryName { get; set; }
    }
}
