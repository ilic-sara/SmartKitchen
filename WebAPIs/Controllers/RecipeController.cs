using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(IRecipeService recipeService, ILogger<RecipeController> logger)
        {
            _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/<RecipeController>
        [HttpGet]
        public async Task<ActionResult<List<Recipe>>> GetAll()
        {
            try
            {
                var recipes = await _recipeService.GetAllAsync();
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAll :: An error occurred while getting all recipes.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }

        }

        // GET api/<RecipeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetById(string id)
        {
            try
            {
                var item = await _recipeService.GetOneByIdAsync(id);
                if (item is null)
                {
                    _logger.LogWarning($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[WARNING] GetById :: Recipe with Id {id} not found.\n", id);
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetById :: An error occurred while getting a Recipe with Id {id}\n{ex}\n.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/<RecipeController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Recipe>> Post([FromBody] Recipe recipe)
        {
            try
            {
                var createdRecipe = await _recipeService.InsertAsync(recipe);
                return CreatedAtAction(nameof(GetById), new { id = createdRecipe }, recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while creating a new Recipe.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<RecipeController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Put([FromBody] Recipe recipe)
        {
            try
            {
                bool success = await _recipeService.UpdateAsync(recipe);
                if (success)
                    return NoContent();
                throw new Exception("0 recipes were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Put :: An error occurred while updating a Recipe with Id {recipe.Id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<RecipeController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                bool success = await _recipeService.DeleteAsync(id);
                if (success)
                    return NoContent();
                throw new Exception("0 recipes were deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Delete :: An error occurred while deleting a Recipe with Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("category")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByCategory([FromQuery] string id, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetRecipesByCategory(id, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByCategory :: An error occurred while fetching Recipes with Category id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("cuisine")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByCuisine([FromQuery] string id, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetRecipesByCuisine(id, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByCuisine :: An error occurred while fetching Recipes with Cuisine id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("diet")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByDiet([FromQuery] string id, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetRecipesByDiet(id, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByDiet :: An error occurred while fetching Recipes with Diet id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("method")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByMethod([FromQuery] string id, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetRecipesByMethod(id, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByMethod :: An error occurred while fetching Recipes with Method id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("name")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByName([FromQuery] string name, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetRecipesByName(name, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByName :: An error occurred while fetching Recipes with name {name}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ingredients")]
        public async Task<ActionResult<List<Recipe>>> GetRecipesByIngredients([FromQuery] string ingredients, [FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var ingredientsList =  ingredients?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

                var recipes = await _recipeService.GetRecipesByIngredients(ingredientsList, startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetRecipesByIngredients :: An error occurred while fetching Recipes with name {string.Join(", ", ingredients)}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("newest")]
        public async Task<ActionResult<List<Recipe>>> GetNewestRecipes([FromQuery] int startIndex = 0, [FromQuery] int numberOfObjects = 10)
        {
            try
            {
                var recipes = await _recipeService.GetNewestRecipes(startIndex, numberOfObjects);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNewestRecipes :: An error occurred while fetching {numberOfObjects} newest recipes.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/name")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByName([FromQuery] string name)
        {
            try
            {
                var number = await _recipeService.GetNumberOfRecipesByName(name);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByName :: An error occurred while fetching total number of Recipes with name {name}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/category")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByCategory([FromQuery] string id)
        {
            try
            {
                var number = await _recipeService.GetNumberOfRecipesByCategory(id);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByCategory :: An error occurred while fetching total number of Recipes with Category Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/cuisine")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByCuisine([FromQuery] string id)
        {
            try
            {
                var number = await _recipeService.GetNumberOfRecipesByCuisine(id);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByCuisine :: An error occurred while fetching total number of Recipes with Cuisine Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/diet")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByDiet([FromQuery] string id)
        {
            try
            {
                var number = await _recipeService.GetNumberOfRecipesByDiet(id);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByDiet :: An error occurred while fetching total number of Recipes with Diet Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/method")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByMethod([FromQuery] string id)
        {
            try
            {
                var number = await _recipeService.GetNumberOfRecipesByMethod(id);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByMethod :: An error occurred while fetching total number of Recipes with Method Id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("count/ingredients")]
        public async Task<ActionResult<long>> GetNumberOfRecipesByIngredients([FromQuery] string ingredients)
        {
            try
            {
                var ingredientsList = ingredients?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

                var number = await _recipeService.GetNumberOfRecipesByIngredients(ingredientsList);
                return Ok(number);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetNumberOfRecipesByIngredients :: An error occurred while fetching total number " +
                    $"of Recipes with ingredients {string.Join(", ", ingredients)}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
