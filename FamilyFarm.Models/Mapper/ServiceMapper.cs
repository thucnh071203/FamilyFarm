using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class ServiceMapper
    {
        public Service service { get; set; }
        public CategoryService categoryService { get; set; }
        public MyProfileDTO? Provider { get; set; }
    }
}
