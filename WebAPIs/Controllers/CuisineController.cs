using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuisineController : ControllerBase
    {
        private readonly ICuisineService _cuisineService;
        private readonly ILogger<CuisineController> _logger;

        public CuisineController(ICuisineService cuisineService, ILogger<CuisineController> logger)
        {
            _cuisineService = cuisineService ?? throw new ArgumentNullException(nameof(cuisineService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/<CuisineController>
        [HttpGet]
        public async Task<ActionResult<List<Cuisine>>> GetAll()
        {
            try
            {
                var cuisines = await _cuisineService.GetAllAsync();
                return Ok(cuisines);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAll :: An error occurred while getting all cuisines.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }

        }

        // GET api/<CuisineController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cuisine>> GetById(string id)
        {
            try
            {
                var item = await _cuisineService.GetOneByIdAsync(id);
                if (item is null)
                {
                    _logger.LogWarning($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[WARNING] GetById :: Cuisine with Id {id} not found.\n", id);
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetById :: An error occurred while getting a Cuisine with Id {id}\n{ex}\n.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/<CuisineController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Cuisine>> Post([FromBody] Cuisine cuisine)
        {
            try
            {
                var createdCuisine = await _cuisineService.InsertAsync(cuisine);
                return CreatedAtAction(nameof(GetById), new { id = createdCuisine }, cuisine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while creating a new Cuisine.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<CuisineController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Put([FromBody] Cuisine cuisine)
        {
            try
            {
                bool success = await _cuisineService.UpdateAsync(cuisine);
                if (success)
                    return NoContent();
                throw new Exception("0 cuisines were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Put :: An error occurred while updating a Cuisine with Id {cuisine.Id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<CuisineController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                bool success = await _cuisineService.DeleteAsync(id);
                if (success)
                    return NoContent();
                throw new Exception("0 cuisines were deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Delete :: An error occurred while deleting a Cuisine with Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
