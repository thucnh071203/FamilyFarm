using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Starts a new chat between two users by their UserIds.
        /// If any of the UserIds is invalid, it returns a BadRequest error.
        /// </summary>
        /// <param name="request">The request data containing the User1Id and User2Id for starting the chat.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the created chat object if successful,
        /// or a BadRequest with an error message if the UserIds are invalid.
        /// </returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartChat([FromBody] StartChatRequestDTO request)
        {
            if (!ObjectId.TryParse(request.User1Id, out _) || !ObjectId.TryParse(request.User2Id, out _))
                return BadRequest("Invalid UserId.");

            var chat = await _chatService.StartChatAsync(request.User1Id, request.User2Id);
            return Ok(chat);
        }

        /// <summary>
        /// Retrieves all chats for a specific user by their UserId.
        /// If no chats are found, it returns a NotFound status.
        /// </summary>
        /// <param name="userId">The UserId for which to fetch the chats.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the list of chats if successful,
        /// or a NotFound status if no chats are found for the given UserId.
        /// </returns>
        [HttpGet("get-by-user/{userId}")]
        public async Task<IActionResult> GetUserChats(string userId)
        {
            var chats = await _chatService.GetUserChatsAsync(userId);
            if (chats == null)
                return NotFound("No chats found!");
            return Ok(chats);
        }

        /// <summary>
        /// Searches for chats by a user's full name.
        /// If no chats are found, it returns a NotFound status.
        /// </summary>
        /// <param name="userId">The UserId of the user performing the search.</param>
        /// <param name="fullName">The full name to search for in chats.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the search results if successful,
        /// or a NotFound status if no chats are found for the given full name.
        /// </returns>
        [HttpGet("search-by-fullname/{userId}")]
        public async Task<IActionResult> SearchChats(string userId, [FromQuery] string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return BadRequest("FullName is required.");
            }
            var chats = await _chatService.SearchChatsByFullNameAsync(userId, fullName);

            if (chats == null)
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
        public async Task<IActionResult> SendMessage([FromBody] ChatDetail chatDetail)
        {
            if (chatDetail == null)
                return BadRequest("No message data provided.");

            try
            {
                var result = await _chatService.SendMessageAsync(chatDetail);

                if (result == null)
                {
                    // Nếu không tạo được message, trả về BadRequest.
                    return BadRequest("Failed to send the message.");
                }

                // Nếu thành công, trả về Ok.
                return Ok(result); // hoặc Ok(chatDetail) nếu bạn muốn trả về tin nhắn gốc.
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
        [HttpGet("get-messages/{chatId}")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            var messages = await _chatService.GetChatMessagesAsync(chatId);
            if (messages == null)
                return NotFound("No chats found!");
            return Ok(messages);
        }


        /// <summary>
        /// Marks a specific chat detail (message) as "seen" by updating the "IsSeen" field to true.
        /// This method will return the updated chat detail if successful, or an error message if no matching chat detail is found.
        /// </summary>
        /// <param name="chatDetailId">The ID of the chat detail (message) to mark as seen.</param>
        /// <returns>
        /// Returns an HTTP 200 status with the updated ChatDetail if successful, 
        /// or an HTTP 404 status with an error message if the chat detail is not found.
        /// </returns>
        [HttpPut("seen/{chatDetailId}")]
        public async Task<IActionResult> MarkAsSeen(string chatDetailId)
        {
            var chatDetail = await _chatService.MarkAsSeenAsync(chatDetailId);
            if (chatDetail == null)
                return NotFound("No chat details found!");  // If no chat detail is found, return 404 with an error message.

            return Ok(chatDetail);  // Return the updated chat detail with HTTP 200 status.
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
        [HttpPut("revoke-message/{chatDetailId}")]
        public async Task<IActionResult> RevokeMessage(string chatDetailId)
        {
            var deleted = await _chatService.RevokeChatDetailByIdAsync(chatDetailId);
            if (deleted == null)
                return NotFound("No message found!");  // If no message is found, return NotFound response.

            return Ok(deleted);  // If the message is successfully revoked, return Ok with the updated message.
        }


        /// <summary>
        /// Deletes the chat history for a given chatId.
        /// </summary>
        /// <param name="chatId">The ID of the chat to delete its history.</param>
        /// <returns>Returns an IActionResult indicating success or failure.</returns>
        [HttpDelete("delete-history/{chatId}")]
        public async Task<IActionResult> DeleteChatHistory(string chatId)
        {
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
    }
}
