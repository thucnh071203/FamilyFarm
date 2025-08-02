using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthenticationService _authenService;

        public ChatController(IChatService chatService, IAuthenticationService authenService)
        {
            _chatService = chatService;
            _authenService = authenService;
        }

        [HttpPost("start-chat/{receiverId}")]
        public async Task<IActionResult> StartChats(string receiverId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var chats = await _chatService.StartChatAsync(account.AccId, receiverId);
            if (chats == null)
                return NotFound("No chats found!");

            return Ok(chats);
        }

        /// <summary>
        /// Retrieves all chats for a specific user by their accId.
        /// If no chats are found, it returns a NotFound status.
        /// </summary>
        /// <param name="accId">The accId for which to fetch the chats.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the list of chats if successful,
        /// or a NotFound status if no chats are found for the given accId.
        /// </returns>
        [HttpGet("get-by-user")]
        public async Task<IActionResult> GetUserChats()
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var chats = await _chatService.GetUserChatsAsync(account.AccId);
            if (chats == null || !chats.Success)
                return NotFound(chats ?? new ListChatResponseDTO { Success = false, Message = "No chats found!" });

            return Ok(chats);
        }

        /// <summary>
        /// Searches for chats by a user's full name.
        /// If no chats are found, it returns a NotFound status.
        /// </summary>
        /// <param name="accId">The accId of the user performing the search.</param>
        /// <param name="fullName">The full name to search for in chats.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the search results if successful,
        /// or a NotFound status if no chats are found for the given full name.
        /// </returns>
        [HttpGet("search-by-fullname/{fullName}")]
        public async Task<IActionResult> SearchChatsByFullName(string fullName)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (string.IsNullOrEmpty(fullName))
                return BadRequest("FullName is required.");

            var chats = await _chatService.SearchChatsByFullNameAsync(account.AccId, fullName);
            if (chats == null || !chats.Any())
                return NotFound("No chats found!");

            return Ok(chats);
        }

        /// <summary>
        /// Sends a new message in a chat.
        /// If the message data is invalid or the message cannot be sent, it returns a BadRequest.
        /// </summary>
        /// <param name="chatDetail">The message detail to be sent.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the sent message if successful,
        /// or a BadRequest status if the message cannot be sent.
        /// </returns>
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequestDTO request)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (request == null)
                return BadRequest("No message data provided.");

            var result = await _chatService.SendMessageAsync(account.AccId, request);

            if (!result.Success.GetValueOrDefault())
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all messages in a specific chat based on the chatId.
        /// If no messages are found, it returns a NotFound status.
        /// </summary>
        /// <param name="chatId">The chatId for which to fetch the messages.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the list of messages if successful,
        /// or a NotFound status if no messages are found for the given chatId.
        /// </returns>
        [HttpGet("get-messages/{receiverId}")]
        public async Task<IActionResult> GetMessages(string receiverId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            try
            {
                // Lấy thông tin người dùng từ token
                var account = _authenService.GetDataFromToken();
                if (account == null)
                {
                    return Unauthorized(new ListChatDetailsResponseDTO
                    {
                        Success = false,
                        Message = "Unauthorized access"
                    });
                }

                // Gọi service để lấy danh sách tin nhắn
                var response = await _chatService.GetChatMessagesAsync(account.AccId, receiverId, skip, take);

                // Trả về kết quả
                if (response.Success)
                {
                    return Ok(response);
                }
                return NotFound(response);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, new ListChatDetailsResponseDTO
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Marks all unseen messages in the specified chat as seen by the currently authenticated user.
        /// </summary>
        /// <param name="chatId">The ID of the chat whose messages are to be marked as seen.</param>
        /// <returns>
        /// An IActionResult indicating the result of the operation:
        /// - 200 OK if messages were successfully marked as seen,
        /// - 400 Bad Request if the operation failed,
        /// - 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPut("mark-messages-as-seen/{chatId}")]
        [Authorize]
        public async Task<IActionResult> MarkMessagesAsSeen(string chatId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var success = await _chatService.MarkMessagesAsSeenAsync(chatId, account.AccId);
            if (!success)
                return BadRequest("Failed to mark messages as seen.");

            return Ok("Messages marked as seen.");
        }

        /// <summary>
        /// Revoke (mark as revoked) a specific chat message by its chatDetailId.
        /// This method updates the "IsRevoked" status of the message to true instead of deleting it.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat message to revoke (mark as revoked).</param>
        /// <returns>
        /// Returns an IActionResult indicating the success or failure of the operation. 
        /// If the message is found and successfully revoked, returns Ok with the revoked message.
        /// If no message is found with the provided ID, returns NotFound with a "No message found!" message.
        /// </returns>
        [HttpPut("recall-message/{chatDetailId}")]
        public async Task<IActionResult> RecallChatMessage(string chatDetailId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            var revoked = await _chatService.RecallChatDetailByIdAsync(chatDetailId);
            if (revoked == null)
                return NotFound("No message found!");

            return Ok(revoked);
        }

        /// <summary>
        /// Deletes the chat history for a given chatId.
        /// </summary>
        /// <param name="chatId">The ID of the chat to delete its history.</param>
        /// <returns>Returns an IActionResult indicating success or failure.</returns>
        [HttpDelete("delete-history/{chatId}")]
        public async Task<IActionResult> DeleteChatHistory(string chatId)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(chatId, out _))
                return NotFound("No chats found.");

            try
            {
                await _chatService.DeleteChatHistoryAsync(chatId);
                return Ok("Chat history deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("bot")]
        public async Task<IActionResult> GetChatResponse([FromBody] string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
                return BadRequest("Input is required.");

            // Đảm bảo đang await đúng cách
            var response = await _chatService.GetChatResponseAsync(userInput);
            return Ok(new { reply = response });
        }
    }
}
