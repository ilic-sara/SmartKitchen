using Microsoft.JSInterop;
using Models;
using SarasKitchenApp.Client.Settings;

namespace SarasKitchenApp.Client.ApiServices
{
    public class RecipeApiService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<RecipeApiService> _logger;
        private readonly int _numberOfRecipesPerPage = PaginationSettings.NumberOfRecipesPerPage;

        public RecipeApiService(IJSRuntime jsRuntime, ILogger<RecipeApiService> logger)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Recipe>> GetRecipesAsync()
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                "recipe"
            );

                return responseData is null ? throw new InvalidOperationException("Failed to get all recipes.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesAsync :: An error occured while fetching all recipes.\n{ex}");
                throw;
            }
        }

        public async Task<Recipe> GetRecipeByIdAsync(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<Recipe>(
                "apiClient.get",
                $"recipe/{id}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipe with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipeByIdAsync :: An error occured while fetching recipe with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCategory(string id, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/category?id={id}&?startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes by category with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByCategory :: An error occured while fetching recipes with category id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCuisine(string id, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/cuisine?id={id}&startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes by cuisine with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByCuisine :: An error occured while fetching recipes with cuisine id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByDiet(string id, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/diet?id={id}&startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes by diet with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByDiet :: An error occured while fetching recipes with diet id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByMethod(string id, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/method?id={id}&startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes by method with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByMethod :: An error occured while fetching recipes with method id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByName(string name, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/name?name={name}&startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes with name " + name) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByName :: An error occured while fetching recipes with name {name}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByIngredients(string ingredients, int startIndex = 0)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/ingredients?ingredients={ingredients}&startIndex={startIndex}&numberOfObjects={_numberOfRecipesPerPage}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get recipes by ingredients.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByIngredients :: An error occured while fetching recipes with ingredients {string.Join(", ", ingredients)}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetNewestRecipes(int startIndex = 0, int numberOfObjects = 10)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Recipe>>(
                "apiClient.get",
                $"recipe/newest?startIndex={startIndex}&numberOfObjects={numberOfObjects}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get newest recipes. ") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNewestRecipes :: An error occured while fetching {numberOfObjects} newest recipes.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByName(string name)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long>(
                "apiClient.get",
                $"recipe/count/name?name={name}"
                );

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByName :: An error occured while fetching total number of recipes with name {name}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByCategory(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long>(
                "apiClient.get",
                $"recipe/count/category?id={id}"
                );

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByCategory :: An error occured while fetching total number of recipes with Category id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByCuisine(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long>(
                "apiClient.get",
                $"recipe/count/cuisine?id={id}"
                );

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByCuisine :: An error occured while fetching total number of recipes with Cuisine id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByDiet(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long>(
                "apiClient.get",
                $"recipe/count/diet?id={id}"
                );

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByDiet :: An error occured while fetching total number of recipes with Diet id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByMethod(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long>(
                "apiClient.get",
                $"recipe/count/method?id={id}"
                );

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByMethod :: An error occured while fetching total number of recipes with Method id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByIngredients(string ingredients)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<long?>(
                    "apiClient.get",
                    $"recipe/count/ingredients?ingredients={ingredients}"
                );

                if (responseData is null)
                {
                    throw new InvalidOperationException("Failed to save recipe.");
                }
                return (long)responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByIngredients :: An error occured while fetching total number of recipes" +
                       $" with ingredients {string.Join(", ", ingredients)}.\n{ex}");
                throw;
            }
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<Recipe>(
                    "apiClient.post",
                    "recipe",
                    recipe
                );

                if (responseData is null)
                {
                    throw new InvalidOperationException("Failed to save recipe.");
                }
                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] CreateRecipeAsync :: An error occured while creating recipe with name {recipe.Name}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateRecipeAsync(string id, Recipe recipe)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "apiClient.put",
                    $"recipe/{id}",
                    recipe 
                );
                _logger.LogInformation($"Recipe with ID {id} updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] UpdateRecipeAsync :: An error occured while updating recipe with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteRecipeAsync(string id)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                "apiClient.delete",
                $"recipe/{id}"
            );
                _logger.LogInformation($"Recipe with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] DeleteRecipeAsync :: An error occured while deleting recipe with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
