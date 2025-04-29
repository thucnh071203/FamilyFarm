using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class StartChatRequestDTO
    {
        public required string User1Id { get; set; }
        public required string User2Id { get; set; }
    }
}
