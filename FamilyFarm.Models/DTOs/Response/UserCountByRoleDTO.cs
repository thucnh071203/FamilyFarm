using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UserCountByRoleDTO
    {
        public string RoleName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
    }
}
