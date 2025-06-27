using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Models.DTOs.Request
{
    public class CategoryReactionDTO
    {
            public string ReactionName { get; set; }
            public IFormFile? IconUrl { get; set; }
        }
    }

