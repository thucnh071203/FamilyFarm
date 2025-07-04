using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task<List<SearchHistory>?> GetListByAccId(string accId);
        Task<bool?> AddSearchHistory(SearchHistory search);
        Task<bool?> DeleteSearchHistory(string searchId);
        Task<bool?> DeleteSearchHistoryBySearchKey(string searchKey);
    }
}
