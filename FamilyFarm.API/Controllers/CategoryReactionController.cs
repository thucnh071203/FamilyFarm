using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyFarm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryReactionController : ControllerBase
    {
        private readonly ICategoryReactionService _categoryReactionService;

        public CategoryReactionController(ICategoryReactionService categoryReactionService)
        {
            _categoryReactionService = categoryReactionService;
        }

        /// <summary>
        /// Retrieves all CategoryReactions that are not deleted.
        /// </summary>
        /// <returns>
        /// An IActionResult containing a list of all active CategoryReactions.
        /// Returns 200 OK with the list, even if empty.
        /// </returns>
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryReactionService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("all-available")]
        public async Task<IActionResult> GetAllAvailable()
        {
            var result = await _categoryReactionService.GetAllAvalableAsync();
            return Ok(result);
        }
    }
}