using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MethodController : ControllerBase
    {
        private readonly IMethodService _methodService;
        private readonly ILogger<MethodController> _logger;

        public MethodController(IMethodService methodService, ILogger<MethodController> logger)
        {
            _methodService = methodService ?? throw new ArgumentNullException(nameof(methodService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/<MethodController>
        [HttpGet]
        public async Task<ActionResult<List<Method>>> GetAll()
        {
            try
            {
                var methods = await _methodService.GetAllAsync();
                return Ok(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAll :: An error occurred while getting all methods.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }

        }

        // GET api/<MethodController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Method>> GetById(string id)
        {
            try
            {
                var item = await _methodService.GetOneByIdAsync(id);
                if (item is null)
                {
                    _logger.LogWarning($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[WARNING] GetById :: Method with Id {id} not found.\n", id);
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetById :: An error occurred while getting a Method with Id {id}\n{ex}\n.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/<MethodController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Method>> Post([FromBody] Method method)
        {
            try
            {
                var createdMethod = await _methodService.InsertAsync(method);
                return CreatedAtAction(nameof(GetById), new { id = createdMethod }, method);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while creating a new Method.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<MethodController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Put([FromBody] Method method)
        {
            try
            {
                bool success = await _methodService.UpdateAsync(method);
                if (success)
                    return NoContent();
                throw new Exception("0 methods were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Put :: An error occurred while updating a Method with Id {method.Id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<MethodController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                bool success = await _methodService.DeleteAsync(id);
                if (success)
                    return NoContent();
                throw new Exception("0 methods were deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Delete :: An error occurred while deleting a Method with Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
