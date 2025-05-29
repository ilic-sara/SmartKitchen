using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Services
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetAllAsync();
        Task<List<Recipe>> GetAllRecipesFromLatest(int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetMultipleByIdsAsync(List<string> ids);
        Task<Recipe?> GetOneByIdAsync(string id);
        Task<bool> UpdateAsync(Recipe model);
        Task<string> InsertAsync(Recipe model);
        Task<bool> DeleteAsync(string id);
        Task<List<Recipe>> GetRecipesByCategory(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByCuisine(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByDiet(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByMethod(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByName(string name, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByIngredients(List<string> ingredients, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetNewestRecipes(int startIndex, int numberOfObjects);
        Task<long> GetTotalNumberOfRecipes();
        Task<long> GetNumberOfRecipesByCategory(string id);
        Task<long> GetNumberOfRecipesByCuisine(string id);
        Task<long> GetNumberOfRecipesByDiet(string id);
        Task<long> GetNumberOfRecipesByMethod(string id);
        Task<long> GetNumberOfRecipesByName(string name);
        Task<long> GetNumberOfRecipesByIngredients(List<string> ingredients);
    }


    public class RecipeService(IRecipeRepository recipeRepository, 
                               ILogger<RecipeService> logger, 
                               ClaimsPrincipal user) : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly ILogger<RecipeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ClaimsPrincipal _user = user ?? throw new ArgumentNullException(nameof(user));

        public async Task<List<Recipe>> GetAllAsync()
        {
            try
            {
                var recipes = await _recipeRepository.GetAllAsync();
                return recipes.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Recipes.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetAllRecipesFromLatest(int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetAllRecipesFromLatest(startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllRecipesFromLatest :: An error occured while fetching Recipes with start index {startIndex}" +
                    $" and number of objects {numberOfObjects}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                return await _recipeRepository.GetMultipleByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching Recipes with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<Recipe?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _recipeRepository.GetOneByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching Recipe with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(Recipe model)
        {
            EnsureAdminUser();

            try
            {
                return await _recipeRepository.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting Recipe.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Recipe model)
        {
            EnsureAdminUser();

            try
            {
                return await _recipeRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating Recipe with id {model.Id}.\n{ex}");
                throw;
            }
        }
        public async Task<bool> DeleteAsync(string id)
        {
            EnsureAdminUser();

            try
            {
                return await _recipeRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting Recipe with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCategory(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByCategory(id, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByCategory :: An error occured while fetching Recipes with Category id {id}.\n" +
                    $"Start index: {startIndex}\n" +
                    $"Number of objects: {numberOfObjects}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCuisine(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByCuisine(id, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByCuisine :: An error occured while fetching Recipes with Cuisine id {id}.\n" +
                    $"Start index: {startIndex}\n" +
                    $"Number of objects: {numberOfObjects}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByDiet(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByDiet(id, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByDiet :: An error occured while fetching Recipes with Diet id {id}.\n" +
                    $"Start index: {startIndex}\n" +
                    $"Number of objects: {numberOfObjects}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByMethod(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByMethod(id, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByMethod :: An error occured while fetching Recipes with Method id {id}.\n" +
                    $"Start index: {startIndex}\n" +
                    $"Number of objects: {numberOfObjects}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByName(string name, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByName(name, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByName :: An error occured while fetching Recipes with name {name}.\n" +
                    $"Start index: {startIndex}\n" +
                    $"Number of objects: {numberOfObjects}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByIngredients(List<string> ingredients, int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetRecipesByIngredients(ingredients, startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetRecipesByIngredients :: An error occured while fetching recipes with ingredient {string.Join(", ", ingredients)}\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetNewestRecipes(int startIndex, int numberOfObjects)
        {
            try
            {
                return await _recipeRepository.GetNewestRecipes(startIndex, numberOfObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNewestRecipes :: An error occured while fetching {numberOfObjects} newest recipes.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetTotalNumberOfRecipes()
        {
            try
            {
                return await _recipeRepository.GetNumberOfDocumentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetTotalNumberOfRecipes :: An error occured while fetching total number of Recipes.\n{ex}");
                throw;
            }
        }
        public async Task<long> GetNumberOfRecipesByCategory(string id)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByCategory(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByCategory :: An error occured while fetching total number of Recipes with Category id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByCuisine(string id)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByCuisine(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByCuisine :: An error occured while fetching total number of Recipes with Cuisine id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByDiet(string id)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByDiet(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByDiet :: An error occured while fetching total number of Recipes with Diet id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByMethod(string id)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByMethod(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByMethod :: An error occured while fetching total number of Recipes with Method id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByName(string name)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByName(name);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByName :: An error occured while fetching total number of Recipes with name {name}\n{ex}");
                throw;
            }
        }


        public async Task<long> GetNumberOfRecipesByIngredients(List<string> ingredients)
        {
            try
            {
                return await _recipeRepository.GetNumberOfRecipesByIngredients(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfRecipesByIngredients :: An error occured while fetching total number " +
                    $"of Recipes with ingredients {string.Join(", ", ingredients)}\n{ex}");
                throw;
            }
        }

        private void EnsureAdminUser()
        {
            if (!_user.IsInRole("Admin"))
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    "[ERROR] Unauthorized action.");
                throw new UnauthorizedAccessException("You are not authorized to perform this action.");
            }
        }

    }
}
