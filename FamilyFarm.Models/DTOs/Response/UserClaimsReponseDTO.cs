﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class UserClaimsResponseDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AccId { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
