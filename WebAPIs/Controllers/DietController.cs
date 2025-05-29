using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DietController : ControllerBase
    {
        private readonly IDietService _dietService;
        private readonly ILogger<DietController> _logger;

        public DietController(IDietService dietService, ILogger<DietController> logger)
        {
            _dietService = dietService ?? throw new ArgumentNullException(nameof(dietService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/<DietController>
        [HttpGet]
        public async Task<ActionResult<List<Diet>>> GetAll()
        {
            try
            {
                var diets = await _dietService.GetAllAsync();
                return Ok(diets);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAll :: An error occurred while getting all diets.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }

        }

        // GET api/<DietController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Diet>> GetById(string id)
        {
            try
            {
                var item = await _dietService.GetOneByIdAsync(id);
                if (item is null)
                {
                    _logger.LogWarning($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[WARNING] GetById :: Diet with Id {id} not found.\n", id);
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetById :: An error occurred while getting a Diet with Id {id}\n{ex}\n.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/<DietController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Diet>> Post([FromBody] Diet diet)
        {
            try
            {
                var createdDiet = await _dietService.InsertAsync(diet);
                return CreatedAtAction(nameof(GetById), new { id = createdDiet }, diet);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while creating a new Diet.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<DietController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Put([FromBody] Diet diet)
        {
            try
            {
                bool success = await _dietService.UpdateAsync(diet);
                if (success)
                    return NoContent();
                throw new Exception("0 diets were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Put :: An error occurred while updating a Diet with Id {diet.Id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<DietController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                bool success = await _dietService.DeleteAsync(id);
                if (success)
                    return NoContent();
                throw new Exception("0 diets were deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Delete :: An error occurred while deleting a Diet with Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
