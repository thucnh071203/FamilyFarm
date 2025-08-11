using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ISharePostService
    {
        Task<ListSharePostResponseDTO?> GetSharePostsByAccId(string? accId);
        Task<ListSharePostResponseDTO?> GetSharePostsByPostId(string? postId);
        Task<SharePostResponseDTO?> CreateSharePost(string? accId, SharePostRequestDTO? request);
        Task<SharePostResponseDTO?> UpdateSharePost(string? sharePostId, UpdateSharePostRequestDTO? request);
        Task<SharePostResponseDTO?> SoftDeleteSharePost(string? sharePostId);
        Task<SharePostResponseDTO?> HardDeleteSharePost(string? sharePostId);
        Task<SharePostResponseDTO?> RestoreSharePost(string? sharePostId);
    }
}
