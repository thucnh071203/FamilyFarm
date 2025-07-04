using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class SearchHistoryDAO : SingletonBase
    {
        private readonly IMongoCollection<SearchHistory> _searchHistory;
        public SearchHistoryDAO(IMongoDatabase database)
        {
            _searchHistory = database.GetCollection<SearchHistory>("SearchHistory");
        }
        public async Task<List<SearchHistory>?> GetListByAccId(string accId)
        {

            var filter = Builders<SearchHistory>.Filter.Eq(f => f.AccId, accId)& 
                Builders<SearchHistory>.Filter.Eq(f => f.IsDeleted, false);

            var searchList = await _searchHistory.Find(filter).ToListAsync();
            if (searchList.Count == 0) return null;

            return searchList;
        } 

        public async Task<bool?> AddSearchHistory(SearchHistory search)
        {
            if (search == null)
            {
                return false;
            }
            await _searchHistory.InsertOneAsync(search);
            return true;
        }
        public async Task<bool?> DeleteSearchHistory(string searchId)
        {
            if (searchId == null)
            {
                return false;
            }
            var filter = Builders<SearchHistory>.Filter.Eq(sh => sh.SearchHistoryId, searchId);


            var update = Builders<SearchHistory>.Update.Set(a => a.IsDeleted, true);
            await _searchHistory.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task<bool?> DeleteSearchHistoryBySearchKey(string searchKey)
        {
            if (searchKey == null)
            {
                return false;
            }
            var filter = Builders<SearchHistory>.Filter.Eq(sh => sh.SearchKey, searchKey);

            var update = Builders<SearchHistory>.Update.Set(a => a.IsDeleted, true);
            await _searchHistory.UpdateManyAsync(filter, update);

            return true;
        }
    }
}
