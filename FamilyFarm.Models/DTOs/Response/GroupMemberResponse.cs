using FamilyFarm.Models.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class GroupMemberListResponse
    {
        public string? Message { get; set; }
        public bool? Success { get; set; }
        public List<GroupMemberRequest>? Data { get; set; }
    }
}
