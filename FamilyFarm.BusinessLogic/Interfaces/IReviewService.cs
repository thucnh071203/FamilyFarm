using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IReviewService
    {
        Task<ListReviewResponseDTO> GetByServiceIdAsync(string serviceId);
        Task<ReviewSummaryDTO> GetSummaryByServiceId(string serviceId);
        //Task<Review> GetByIdAsync(string id);
        Task<ReviewResponseDTO> CreateAsync(ReviewRequestDTO request, string accId);
        //Task<ReviewResponseDTO> UpdateAsync(string id, ReviewRequestDTO request, string accId);
        Task<ReviewResponseDTO> DeleteAsync(string id, string accId);
    }
}
