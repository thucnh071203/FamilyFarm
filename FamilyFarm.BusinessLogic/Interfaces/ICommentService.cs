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
    public interface ICommentService
    {
        Task<ListCommentResponseDTO?> GetAllCommentWithReactionByPost(string? postId);   
        Task<CommentResponseDTO> GetById(string id);
        Task<CommentResponseDTO> Create(CommentRequestDTO request, string accId);
        Task<CommentResponseDTO> Update(string id, CommentRequestDTO request, string accId);
        Task<CommentResponseDTO> Delete(string id, string accId);
    }
}
