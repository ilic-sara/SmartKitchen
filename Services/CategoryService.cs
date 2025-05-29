using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using MongoDB.Driver;

namespace Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<List<Category>> GetMultipleByIdsAsync(List<string> ids);
        Task<Category?> GetOneByIdAsync(string id);
        Task<bool> UpdateAsync(Category model);
        Task<string> InsertAsync(Category model);
        Task<bool> DeleteAsync(string id);
    }

    public class CategoryService(ICategoryRepository categoryRepository,
                                 IRecipeRepository recipeRepository,
                                 ILogger<CategoryService> logger,
                                 IMongoClient mongoClient,
                                 ClaimsPrincipal user) : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly ILogger<CategoryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMongoClient _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        private readonly ClaimsPrincipal _user = user ?? throw new ArgumentNullException(nameof(user));

        public async Task<List<Category>> GetAllAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return categories.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Categories.\n{ex}");
                throw;
            }
        }

        public async Task<List<Category>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                return await _categoryRepository.GetMultipleByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching Categories with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<Category?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _categoryRepository.GetOneByIdAsync(id);

            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching Category with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(Category model)
        {
            EnsureAdminUser();

            try
            {
                return await _categoryRepository.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting Category.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Category model)
        {
            EnsureAdminUser();

            using (var session = await _mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    bool success = await _categoryRepository.UpdateAsync(model, session);

                    if(!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.UpdateCategoryInRecipes(model.Id, model.Name, model.PictureUrl, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] UpdateAsync :: An error occured while updating Category with id {model.Id}.\n{ex}");

                    await session.AbortTransactionAsync();

                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            EnsureAdminUser();

            using (var session = await _mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    bool success = await _categoryRepository.DeleteAsync(id);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.DeleteCategoryInRecipes(id, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] DeleteAsync :: An error occured while deleting Category with id {id}.\n{ex}");

                    await session.AbortTransactionAsync();

                    throw;
                }
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
