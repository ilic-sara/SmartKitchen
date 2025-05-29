using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/<CategoryController>
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[INFO] GetAll :: Received a request to get all categories.");
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " + 
                    $"[ERROR] GetAll :: An error occurred while getting all categories.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
            
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById(string id)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[INFO] GetById :: Received a request to get category by Id.");
                var item = await _categoryService.GetOneByIdAsync(id);
                if (item is null)
                {
                    _logger.LogWarning($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[WARNING] GetById :: Category with Id {id} not found.\n", id);
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " + 
                    $"[ERROR] GetById :: An error occurred while getting a Category with Id {id}\n{ex}\n.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/<CategoryController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Category>> Post([FromBody] Category category)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[INFO] Post :: Received a request to create category.");
                var createdCategory = await _categoryService.InsertAsync(category);
                return CreatedAtAction(nameof(GetById), new { id = createdCategory }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while creating a new Category.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Put([FromBody] Category category)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[INFO] Put :: Received a request to update category.");
                bool success = await _categoryService.UpdateAsync(category);
                if(success)
                    return NoContent();
                throw new Exception("0 categories were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Put :: An error occurred while updating a Category with Id {category.Id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[INFO] Delete :: Received a request to delete category.");
                bool success = await _categoryService.DeleteAsync(id);
                if(success)
                    return NoContent();
                throw new Exception("0 categories were deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Delete :: An error occurred while deleting a Category with Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
