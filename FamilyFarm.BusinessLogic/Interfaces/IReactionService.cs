using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IReactionService
    {
        Task<bool> ToggleReactionAsync(string entityId, string entityType, string accId, string categoryReactionId);
        Task<ListReactionResponseDTO> GetAllByEntityAsync(string entityId, string entityType);
    }
}
