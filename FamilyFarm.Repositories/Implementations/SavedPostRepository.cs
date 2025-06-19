using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class SavedPostRepository : ISavedPostRepository
    {
        private readonly SavedPostDAO _savedPostDAO;

        public SavedPostRepository(SavedPostDAO savedPostDAO)
        {
            _savedPostDAO = savedPostDAO;
        }

        public async Task<bool?> CheckSavedPost(string? accId, string? postId)
        {
            return await _savedPostDAO.IsSavedPost(accId, postId);
        }

        public async Task<SavedPost?> CreateSavedPost(SavedPost? request)
        {
            return await _savedPostDAO.CreateAsync(request);
        }

        public async Task<bool?> DeleteSavedPost(string? accId, string? postId)
        {
            return await _savedPostDAO.DeleteSavedPost(accId, postId);
        }

        public async Task<List<SavedPost>?> ListSavedPostOfAccount(string? accId)
        {
            return await _savedPostDAO.GetListAvailableByAccount(accId);
        }
    }
}
