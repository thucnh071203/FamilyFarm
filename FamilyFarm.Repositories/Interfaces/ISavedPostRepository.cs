using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface ISavedPostRepository
    {
        Task<SavedPost?> CreateSavedPost(SavedPost? request); //Tạo mới 1 saved post
        Task<List<SavedPost>?> ListSavedPostOfAccount(string? accId); //Lấy danh sách saved post của 1 account
        Task<bool?> CheckSavedPost(string? accId, string? postId);
        Task<bool?> DeleteSavedPost(string? accId, string? postId);
    }
}
