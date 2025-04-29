using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class SearchHistoryResponseDTO
    {
        public bool Success { get; set; }             // true if register successful
        public string? MessageError { get; set; }
        public List<SearchHistory>? Data { get; set; }// Message if has error
    }
}
