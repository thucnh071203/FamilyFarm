using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ISavedPostService
    {
        Task<CreatedSavedPostResponseDTO?> SavedPost(string? accId, string? postId);
        Task<ListPostResponseDTO?> ListSavedPostOfAccount(string? accId);
        Task<bool?> CheckIsSavedPost(string? accId, string? postId);
        Task<bool?> UnsavedPost(string? accId, string? postId);
    }
}
