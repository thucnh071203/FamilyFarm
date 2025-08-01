﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UserInGroupDTO
    {
        public string AccId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string City { get; set; }
        public DateTime? JoinAt { get; set; }
    }
}
