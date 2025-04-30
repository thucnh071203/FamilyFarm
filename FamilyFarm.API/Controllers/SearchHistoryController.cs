using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/[controller]")]
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
        [HttpGet("list-search-history")]
        [Authorize]
        public async Task<ActionResult> GetListSearchHistory() {
            var userClaims = _authenService.GetDataFromToken();
            var accId = userClaims?.AccId;
            var result = await _searchHistoryService.GetListByAccId(accId);

            if (result.Success==false)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("add-search-history/{searchKey}")]
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
        [HttpDelete("delete-search-history/{searchId}")]
        [Authorize]
        public async Task<ActionResult> DeleteSearchHistory(string searchId)
        {
            var result = await _searchHistoryService.DeleteSearchHistory(searchId);

            if (result == false)
                return BadRequest();

            return Ok(result);
        }
    }
}
