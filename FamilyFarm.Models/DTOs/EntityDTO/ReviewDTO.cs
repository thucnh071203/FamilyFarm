using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class ReviewDTO
    {
        public Review Review { get; set; }
        public MyProfileDTO Reviewer { get; set; }
    }
}
