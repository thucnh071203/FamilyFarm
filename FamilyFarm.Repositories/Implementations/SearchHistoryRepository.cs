using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly SearchHistoryDAO searchHistoryDAO;

        public SearchHistoryRepository(SearchHistoryDAO searchHistoryDAO)
        {
            this.searchHistoryDAO = searchHistoryDAO;
        }

        public async Task<bool?> AddSearchHistory(SearchHistory search)
        {
           return await searchHistoryDAO.AddSearchHistory(search);
        }

        public async Task<bool?> DeleteSearchHistory(string searchId)
        {
            return await searchHistoryDAO.DeleteSearchHistory(searchId);
        }

        public async Task<bool?> DeleteSearchHistoryBySearchKey(string searchKey)
        {
            return await searchHistoryDAO.DeleteSearchHistoryBySearchKey(searchKey);
        }

        public async Task<List<SearchHistory>?> GetListByAccId(string accId)
        {
            return await searchHistoryDAO.GetListByAccId(accId);
        }
    }
}
