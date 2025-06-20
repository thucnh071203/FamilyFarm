using AutoMapper;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FamilyFarm.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticationService _authenService;
        private readonly IMapper _mapper;

        public AccountController(IAccountService accountService, IAuthenticationService authenService, IMapper mapper)
        {
            _accountService = accountService;
            _authenService = authenService;
            _mapper = mapper;
        }

        /*[HttpPut("update-profile-farmer/{username}")]
        public async Task<IActionResult> UpdateProfileFarmer(string username, UpdateProfileRequestDTO updateProfile)
        {
            if (string.IsNullOrEmpty(updateProfile.FullName) ||
                    updateProfile.Birthday == null)
            {
                return BadRequest("Information not be blank");
            }

            updateProfile.Gender = updateProfile.Gender = string.IsNullOrWhiteSpace(updateProfile.Gender) ? "Not specified" : updateProfile.Gender;
            updateProfile.City = string.IsNullOrWhiteSpace(updateProfile.City) ? "" : updateProfile.City;
            updateProfile.Country = string.IsNullOrWhiteSpace(updateProfile.Country) ? "" : updateProfile.Country;
            updateProfile.Address = string.IsNullOrWhiteSpace(updateProfile.Address) ? null : updateProfile.Address;
            updateProfile.Background = string.IsNullOrWhiteSpace(updateProfile.Background) ? null : updateProfile.Background;
            updateProfile.Certificate = null;
            updateProfile.WorkAt = null;
            updateProfile.StudyAt = null;

            var result = await _accountService.UpdateProfileAsync(username, updateProfile);

            if (result == null)
            {
                return StatusCode(500);
            } else if (!result.IsSuccess)
            {
                return StatusCode(400, result);
            }

            return Ok(result);
        }

        [HttpPut("update-profile-expert/{username}")]
        public async Task<IActionResult> UpdateProfileExpert(string username, UpdateProfileRequestDTO updateProfile)
        {
            if (string.IsNullOrEmpty(updateProfile.FullName) ||
                    updateProfile.Birthday == null ||
                    string.IsNullOrEmpty(updateProfile.Certificate) ||
                    string.IsNullOrEmpty(updateProfile.WorkAt) ||
                    string.IsNullOrEmpty(updateProfile.StudyAt))
            {
                return BadRequest("Information not be blank");
            }

            updateProfile.Gender = updateProfile.Gender = string.IsNullOrWhiteSpace(updateProfile.Gender) ? "Not specified" : updateProfile.Gender;
            updateProfile.City = string.IsNullOrWhiteSpace(updateProfile.City) ? "" : updateProfile.City;
            updateProfile.Country = string.IsNullOrWhiteSpace(updateProfile.Country) ? "" : updateProfile.Country;
            updateProfile.Address = string.IsNullOrWhiteSpace(updateProfile.Address) ? null : updateProfile.Address;
            updateProfile.Background = string.IsNullOrWhiteSpace(updateProfile.Background) ? null : updateProfile.Background;

            var result = await _accountService.UpdateProfileAsync(username, updateProfile);

            if (result == null)
            {
                return StatusCode(500);
            }
            else if (!result.IsSuccess)
            {
                return StatusCode(400, result);
            }

            return Ok(result);
        }*/
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequestDTO updateProfile)
        {
            var account = _authenService.GetDataFromToken();
            if (account == null)
                return Unauthorized("Invalid token or user not found.");

            if (!ObjectId.TryParse(account.AccId, out _))
                return BadRequest("Invalid AccIds.");

            if (account == null)
            {
                return BadRequest("Account not found");
            }

            if (string.IsNullOrEmpty(updateProfile.FullName))
            {
                return BadRequest("Basic information not be blank");
            }

            // Check acc có phải expert không?

            if (account.RoleId.Equals("68007b2a87b41211f0af1d57"))
            {
                //if (updateProfile.Certificate == null ||
                //    string.IsNullOrEmpty(updateProfile.WorkAt) ||
                //    string.IsNullOrEmpty(updateProfile.StudyAt))
                //{
                //    return BadRequest("Expert information not be blank");
                //}

                var result = await _accountService.UpdateProfileAsync(account.AccId, updateProfile);

                Console.WriteLine("UpdateProfile Result:");
                Console.WriteLine($"IsSuccess: {result?.IsSuccess}, Message: {result?.MessageError}");

                if (result == null)
                {
                    return StatusCode(500);
                }
                else if (!result.IsSuccess)
                {
                    return StatusCode(400, result);
                }

                return Ok(result);
            }
            else
            {
                var result = await _accountService.UpdateProfileAsync(account.AccId, updateProfile);

                if (result == null)
                {
                    return StatusCode(500);
                }
                else if (!result.IsSuccess)
                {
                    return StatusCode(400, result);
                }

                return Ok(result);
            }
        }


        [HttpGet("profile-another/{accId}")]
        public async Task<IActionResult> GetUserProfile(string accId)
        {
            var profile = await _accountService.GetUserProfileAsync(accId);

            if (profile == null)
                return NotFound(new
                {
                    message = "User not found",
                    success = false
                });

            return Ok(new
            {
                message = "User profile found",
                success = true,
                data = profile
            });
        }

        [HttpGet("own-profile")]
        [Authorize]
        public async Task<ActionResult<MyProfileResponseDTO>> GetMyProfile()
        {
            var userClaims = _authenService.GetDataFromToken();
            var username = userClaims?.Username;

            if (username == null)
                return Unauthorized("Not permission for this action.");

            var account = await _accountService.GetAccountByUsername(username);

            if (account == null)
                return BadRequest("Error encountered during execution.");

            var data = _mapper.Map<MyProfileDTO>(account);

            return Ok(new MyProfileResponseDTO
            {
                Message = "Get own profile success.",
                Success = true,
                Data = data
            });
        }

        [HttpPut("change-avatar")]
        [Authorize]
        public async Task<ActionResult<UpdateAvatarResponseDTO>> ChangeAvatar([FromForm] UpdateAvatarRequesDTO request)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accountId = userClaims?.AccId;

            if (accountId == null)
                return Unauthorized("Not permission for this action.");

            var result = await _accountService.ChangeOwnAvatar(accountId, request);
            if(result == null)
                return BadRequest("Error encountered during execution.");

            return Ok(result);
        }

        [HttpGet("list-censor/{role_id}")]
        public async Task<IActionResult> GetAllAccountByRoleId(string role_id)
        {
            var list = await _accountService.GetAllAccountByRoleId(role_id);

            if (list == null)
                return NotFound(new
                {
                    message = "list not found",
                    success = false
                });

            return Ok(list);
        }

        [HttpPut("update-censor/{accId}/{status}")]
        public async Task<IActionResult> UpdateAccountStatus(string accId, int status)
        {
            var update = await _accountService.UpdateAccountStatus(accId, status);
            if (update != true)
            {
                return BadRequest("have some error when update status of censor!");
            }

            return Ok(update);
        }
        [HttpGet("get-by-accId/{accId}")]
        public async Task<IActionResult> GetAccountByAccId(string accId)
        {
            var update = await _accountService.GetAccountByAccId(accId);
            if (update == null)
            {
                return BadRequest("have some error when get account information!");
            }

            return Ok(update);
        }


    }
}
