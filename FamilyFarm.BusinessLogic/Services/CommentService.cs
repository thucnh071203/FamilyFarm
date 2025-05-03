using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all comments associated with a specific post.
        /// </summary>
        /// <param name="postId">The ID of the post to fetch comments for.</param>
        /// <returns>A list of comments belonging to the specified post.</returns>
        public async Task<List<Comment>> GetAllByPost(string postId)
        {
            return await _commentRepository.GetAllByPost(postId);
        }

        /// <summary>
        /// Retrieves a comment by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the comment.</param>
        /// <returns>A CommentResponseDTO containing comment data or an error message.</returns>
        public async Task<CommentResponseDTO> GetById(string id)
        {
            // Validate comment ID format
            if (!ObjectId.TryParse(id, out _))
                return new CommentResponseDTO { Success = false, Message = "Invalid Comment ID" };

            // Fetch the comment from the repository
            var comment = await _commentRepository.GetById(id);
            if (comment == null)
                return new CommentResponseDTO { Success = false, Message = "Comment not found" };

            // Map the comment entity to a response DTO
            var response = _mapper.Map<CommentResponseDTO>(comment);
            response.Success = true;
            response.Message = "Comment retrieved successfully";
            return response;
        }

        /// <summary>
        /// Creates a new comment for a specific post by a specific account.
        /// </summary>
        /// <param name="request">The comment data from the client.</param>
        /// <param name="accId">The ID of the account making the comment.</param>
        /// <returns>A CommentResponseDTO indicating success or failure.</returns>
        public async Task<CommentResponseDTO> Create(CommentRequestDTO request, string accId)
        {
            // Basic validation for request data
            if (request == null || string.IsNullOrEmpty(request.PostId) || string.IsNullOrEmpty(request.Content))
                return new CommentResponseDTO { Success = false, Message = "Invalid comment data" };

            // Validate object ID formats
            if (!ObjectId.TryParse(request.PostId, out _) || !ObjectId.TryParse(accId, out _))
                return new CommentResponseDTO { Success = false, Message = "Invalid Post ID or Account ID" };

            // Map request DTO to Comment entity and set the account ID
            var comment = _mapper.Map<Comment>(request);
            comment.AccId = accId;

            // Save the new comment to the database
            var createdComment = await _commentRepository.Create(comment);
            if (createdComment == null)
                return new CommentResponseDTO { Success = false, Message = "Failed to create comment" };

            // Return success response with created comment details
            var response = _mapper.Map<CommentResponseDTO>(createdComment);
            response.Success = true;
            response.Message = "Comment created successfully";
            return response;
        }

        /// <summary>
        /// Updates the content of an existing comment if it belongs to the specified account.
        /// </summary>
        /// <param name="id">The ID of the comment to update.</param>
        /// <param name="request">The new comment data from the client.</param>
        /// <param name="accId">The ID of the account attempting the update.</param>
        /// <returns>A CommentResponseDTO indicating success or failure.</returns>
        public async Task<CommentResponseDTO> Update(string id, CommentRequestDTO request, string accId)
        {
            // Validate the request content
            if (request == null || string.IsNullOrEmpty(request.Content))
                return new CommentResponseDTO { Success = false, Message = "Invalid comment data" };

            // Retrieve the comment and ensure it belongs to the requesting account
            var existingComment = await _commentRepository.GetById(id);
            if (existingComment == null || existingComment.AccId != accId)
                return new CommentResponseDTO { Success = false, Message = "Comment not found" };

            // Update the content and save changes
            existingComment.Content = request.Content;
            var updatedComment = await _commentRepository.Update(id, existingComment);
            if (updatedComment == null)
                return new CommentResponseDTO { Success = false, Message = "Failed to update comment" };

            // Return success response
            var response = _mapper.Map<CommentResponseDTO>(updatedComment);
            response.Success = true;
            response.Message = "Comment updated successfully";
            return response;
        }

        /// <summary>
        /// Deletes a comment if it belongs to the specified account.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <param name="accId">The ID of the account requesting deletion.</param>
        /// <returns>A CommentResponseDTO indicating success or failure.</returns>
        public async Task<CommentResponseDTO> Delete(string id, string accId)
        {
            // Validate the comment ID format
            if (!ObjectId.TryParse(id, out _))
                return new CommentResponseDTO { Success = false, Message = "Invalid Comment ID" };

            // Retrieve the comment and ensure it belongs to the account
            var existingComment = await _commentRepository.GetById(id);
            if (existingComment == null || existingComment.AccId != accId)
                return new CommentResponseDTO { Success = false, Message = "Comment not found" };

            // Delete the comment
            await _commentRepository.Delete(id);
            return new CommentResponseDTO { Success = true, Message = "Comment deleted successfully" };
        }
    }
}
