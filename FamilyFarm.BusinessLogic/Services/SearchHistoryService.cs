﻿using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly ISearchHistoryRepository _repository;
        public SearchHistoryService(ISearchHistoryRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool?> AddSearchHistory(string accId, string searchKey)
        {
            if (string.IsNullOrEmpty(searchKey)) return null;
            var searchHistory = new SearchHistory
            {
                SearchHistoryId = "",
                AccId = accId,
                SearchKey = searchKey,
                SearchedAt = DateTime.Now,
                IsDeleted = false,
            };
            return await _repository.AddSearchHistory(searchHistory);
        }

        public async Task<bool?> DeleteSearchHistory(string searchId)
        {
            if (string.IsNullOrEmpty(searchId)) return null;
            var result = await _repository.DeleteSearchHistory(searchId);
            return result;
        }

        public async Task<SearchHistoryResponseDTO> GetListByAccId(string accId)
        {
            if (string.IsNullOrEmpty(accId)) return null;
            var list = await _repository.GetListByAccId(accId);
            if (list==null || list.Count ==0) return new SearchHistoryResponseDTO
            {
                Success = false,
                MessageError = "You dont have any search history",
            };
            return new SearchHistoryResponseDTO
            {
                Success = true,
                Data = list
            };
        }
    }
}
