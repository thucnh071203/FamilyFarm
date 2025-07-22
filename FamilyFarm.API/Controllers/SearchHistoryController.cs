using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/search-history")]
    [ApiController]
    public class SearchHistoryController : ControllerBase
    {
        private readonly ISearchHistoryService _searchHistoryService;
        private readonly IAuthenticationService _authenService;
        public SearchHistoryController(ISearchHistoryService searchHistoryService, IAuthenticationService authenService)
        {
            _searchHistoryService = searchHistoryService;
            _authenService = authenService;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult> GetListSearchHistory() {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            if (accId == null)  return Unauthorized();
            var result = await _searchHistoryService.GetListByAccId(accId);

            if (result.Success==false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list-no-duplicate")]
        [Authorize]
        public async Task<ActionResult> GetListSearchHistoryNoDuplicate()
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _searchHistoryService.GetListByAccIdNoDuplicate(accId);

            if (result.Success == false)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("create/{searchKey}")]
        [Authorize]
        public async Task<ActionResult> AddSearchHistory(string searchKey)
        {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _searchHistoryService.AddSearchHistory(accId, searchKey);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }

        [HttpDelete("delete/{searchId}")]
        [Authorize]
        public async Task<ActionResult> DeleteSearchHistory(string searchId)
        {
            var result = await _searchHistoryService.DeleteSearchHistory(searchId);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }

        [HttpDelete("delete-by-search-key/{searchKey}")]
        [Authorize]
        public async Task<ActionResult> DeleteSearchHistoryBySearchKey(string searchKey)
        {
            var result = await _searchHistoryService.DeleteSearchHistoryBySearchKey(searchKey);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }
    }
}
