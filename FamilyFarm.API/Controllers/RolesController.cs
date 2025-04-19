using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly RoleDAO _service;

        public RolesController(RoleDAO service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> Get(string id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Role product)
        {
            await _service.CreateAsync(product);
            return CreatedAtAction(nameof(Get), new { id = product.RoleId }, product);
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(string id, Role product)
        //{
        //    var existing = await _service.GetByIdAsync(id);
        //    if (existing == null) return NotFound();

        //    product.RoleId = id;
        //    await _service.UpdateAsync(id, product);
        //    return NoContent();
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
