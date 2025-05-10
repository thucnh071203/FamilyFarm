using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UserByProvinceResponseDTO
    {
        public string Province { get; set; }
        public int UserCount { get; set; }
    }
}
