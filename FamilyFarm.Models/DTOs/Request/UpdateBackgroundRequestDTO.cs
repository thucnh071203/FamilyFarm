﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class UpdateBackgroundRequestDTO
    {
        [FromForm(Name = "NewBackground")]
        public IFormFile? NewBackground { get; set; }
    }
}
