using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdateAvatarRequesDTO
    {
        [FromForm(Name = "NewAvatar")]
        public IFormFile? NewAvatar {  get; set; }
    }
}
