using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ISearchHistoryService
    {
        Task<SearchHistoryResponseDTO> GetListByAccId(string accId);
        Task<SearchHistoryResponseDTO> GetListByAccIdNoDuplicate(string accId);
        Task<bool?> AddSearchHistory(string accId, string searchKey);
        Task<bool?> DeleteSearchHistory(string searchId);
        Task<bool?> DeleteSearchHistoryBySearchKey(string searchKey);
    }
}
