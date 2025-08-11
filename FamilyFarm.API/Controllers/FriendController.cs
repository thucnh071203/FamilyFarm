using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FamilyFarm.API.Controllers
{
    [Route("api/friend")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRequestService _friendService;
        private readonly IFriendService _serviceOfFriend;
        private readonly IAccountService _accountService;
        private readonly IAuthenticationService _authenService;



        public FriendController(IFriendRequestService friendService, IFriendService serviceOfFriend, IAccountService accountService, IAuthenticationService authenService)
        {
            _friendService = friendService;
            _serviceOfFriend = serviceOfFriend;
            _accountService = accountService;
            _authenService = authenService;
        }

        /// <summary>
        /// Retrieves the list of friend requests SENT by a user that are still in PENDING status.
        /// </summary>
        /// <param name="userId">The ID of the user who sent the friend requests.</param>
        /// <returns>A list of pending sent friend requests, or an empty list if none found.</returns>
        [HttpGet("requests-sent")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetSendRequest()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            var result = await _friendService.GetAllSendFriendRequests(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the list of friend requests RECEIVED by a user that are still in PENDING status.
        /// </summary>
        /// <param name="userId">The ID of the user who received the friend requests.</param>
        /// <returns>A list of pending received friend requests, or an empty list if none found.</returns>


        [HttpGet("requests-receive")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetReceiveRequest()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;
            if(username == null) return Unauthorized();

            var result = await _friendService.GetAllReceiveFriendRequests(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("accept/{otherId}")]
        [Authorize]
        public async Task<IActionResult> AcceptFriendRequest(string otherId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if(accId == null) return Unauthorized();
            var result = await _friendService.AcceptFriendRequestAsync(accId, otherId);
            if (result)
            {
                return Ok("Friend request accepted.");
            }

            return BadRequest("Friend request could not be accepted.");
        }

        // API Từ chối yêu cầu kết bạn
        [HttpPost("reject/{otherId}")]
        [Authorize]
        public async Task<IActionResult> RejectFriendRequest(string otherId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null) return Unauthorized();
            var result = await _friendService.RejectFriendRequestAsync(accId, otherId);
            if (result)
            {
                return Ok("Friend request rejected.");
            }

            return BadRequest("Friend request could not be rejected.");
        }


        /// <summary>
        /// trả lời những lời mời kết bạn nhận được in PENDING status.
        /// </summary>
        /// <param name="request">Nội dung phản hồi gồm FriendID và accept hay reject.</param>
        /// <returns>A list of pending friend requests, và người dùng sẽ accept hay reject.</returns>


        //[HttpPost("respond-request")]
        //public async Task<IActionResult> RespondToFriendRequest([FromBody] FriendRequestDTO request)
        //{
        //    if (request.Action != "Accept" && request.Action != "Reject")
        //    {
        //        return BadRequest(new FriendRequestResponse
        //        {
        //            Message = "Action phải là 'accept' hoặc 'reject'.",
        //            IsSuccess = false
        //        });
        //    }

        //    bool result = request.Action == "Accept"
        //        ? await _friendService.AcceptFriendRequestAsync(request.FriendId)
        //        : await _friendService.RejectFriendRequestAsync(request.FriendId);

        //    if (result)
        //    {
        //        return Ok(new FriendRequestResponse
        //        {
        //            Message = "Yêu cầu đã được xử lý thành công.",
        //            IsSuccess = true
        //        });
        //    }
        //    else
        //    {
        //        return StatusCode(500, new FriendRequestResponse
        //        {
        //            Message = "Có lỗi xảy ra khi xử lý yêu cầu.",
        //            IsSuccess = false
        //        });
        //    }
        //}

        [HttpPost("send-friend-request")]
        [Authorize]
        public async Task<IActionResult> SendFriendRequest([FromBody] CreateFriendRequestDTO request)
        {
            //var senderId = User.Identity.Name; // Giả sử bạn có thể lấy senderId từ authentication context.

            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;
            if(username == null) return Unauthorized();

            var user = await _accountService.GetAccountByUsername(username); // Truy vấn người dùng theo username

            if (user == null)
            {
                return BadRequest("Không tìm thấy người dùng.");
            }

            var sendId = user.AccId; // Lấy AccID từ user tìm được


            var receiverId = request.ReceiverId;

            if (string.IsNullOrEmpty(receiverId))
            {
                return BadRequest("ReceiverId không hợp lệ.");
            }

            var result = await _friendService.SendFriendRequestAsync(sendId, receiverId);

            if (result)
            {

                return Ok(new FriendRequestResponse
                {
                    Message = "Yêu cầu kết bạn đã được gửi.",
                    IsSuccess = true
                });
            }
            else
            {
                return BadRequest(new FriendRequestResponse
                {
                    Message = "Fail.",
                    IsSuccess = false
                });
            }
        }

        [HttpGet("list-friend")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFriends()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null) return Unauthorized();

            var result = await _serviceOfFriend.GetListFriends(accId);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-friend-other/{accIdOfOther}")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFriends(string accIdOfOther)
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;
            if (username == null) return Unauthorized();

            var result = await _serviceOfFriend.GetListFriends(accIdOfOther);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-follower")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFollower()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            var result = await _serviceOfFriend.GetListFollower(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("list-following")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListFollowing()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            var result = await _serviceOfFriend.GetListFollowing(username);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("unfriend/{accId}")]
        [Authorize]
        public async Task<ActionResult> Unfriend(string accId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accountId = userClaims?.AccId;
            if (accountId == null) return Unauthorized();

            var result = await _serviceOfFriend.Unfriend(accountId, accId);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("mutual-friend/{otherId}")]
        [Authorize]
        public async Task<ActionResult<FriendResponseDTO>> GetListMutualFriend(string otherId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.AccId;

            var result = await _serviceOfFriend.MutualFriend(username, otherId);

            if (result == null)
                return BadRequest();

            if (result.IsSuccess == false)
                return NotFound(result);

            return Ok(result);
        }

        //get list suggestion friend in Friend and profile
        [HttpGet("suggestion-friend")]
        [Authorize]
        public async Task<ActionResult<List<Account>>> GetListSuggestionFriends()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _serviceOfFriend.GetListSuggestionFriends(accId, 50);

            return Ok(result);


        }

        //get list suggestion friend in Friend and profile
        [HttpGet("suggestion-friend-home")]
        [Authorize]
        public async Task<ActionResult<List<Account>>> GetListSuggestionFriendsHome()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _serviceOfFriend.GetListSuggestionFriends(accId, 4);

            return Ok(result);


        }

        //get list suggestion friend in Friend and profile
        [HttpGet("suggestion-expert")]
        [Authorize]
        public async Task<ActionResult<List<Account>>> GetSuggestedExperts()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _serviceOfFriend.GetSuggestedExperts(accId, 6);

            return Ok(result);


        }

        //get list suggestion friend in Friend and profile
        [HttpGet("list-account-no-relation")]
        [Authorize]
        public async Task<ActionResult<List<FriendResponseDTO>>> GetAvailableFarmersAndExperts()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _serviceOfFriend.GetAvailableFarmersAndExpertsAsync(accId);

            return Ok(result);


        }

        [HttpGet("check-is-friend")]
        public async Task<IActionResult> CheckIsFriend([FromQuery] string receiverId)
        {
            var userClaims = _authenService.GetDataFromToken();
            var senderId = userClaims?.AccId;

            try
            {
                var status = await _serviceOfFriend.CheckIsFriendAsync(senderId, receiverId);
                return Ok(new { status });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("search-users")]
        public async Task<ActionResult<FriendResponseDTO>> SearchUsers([FromQuery] string keyword, [FromQuery] int number = 50)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _serviceOfFriend.SearchUsers(accId, keyword, number);
            return Ok(result);
        }
    }
}
