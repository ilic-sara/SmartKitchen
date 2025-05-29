using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Security.Claims;

namespace Services
{
    public interface ICuisineService
    {
        Task<List<Cuisine>> GetAllAsync();
        Task<List<Cuisine>> GetMultipleByIdsAsync(List<string> ids);
        Task<Cuisine?> GetOneByIdAsync(string id);
        Task<bool> UpdateAsync(Cuisine model);
        Task<string> InsertAsync(Cuisine model);
        Task<bool> DeleteAsync(string id);
    }


    public class CuisineService(ICuisineRepository cuisineRepository,
                                IRecipeRepository recipeRepository,
                                IMongoClient mongoClient,
                                ILogger<CuisineService> logger,
                                ClaimsPrincipal user) : ICuisineService
    {
        private readonly ICuisineRepository _cuisineRepository = cuisineRepository ?? throw new ArgumentNullException(nameof(cuisineRepository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly IMongoClient _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        private readonly ILogger<CuisineService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ClaimsPrincipal _user = user ?? throw new ArgumentNullException(nameof(user));

        public async Task<List<Cuisine>> GetAllAsync()
        {
            try
            {
                var cuisines = await _cuisineRepository.GetAllAsync();
                return cuisines.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Cuisines.\n{ex}");
                throw;
            }
        }

        public async Task<List<Cuisine>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                return await _cuisineRepository.GetMultipleByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching Cuisines with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<Cuisine?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _cuisineRepository.GetOneByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching Cuisine with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(Cuisine model)
        {
            EnsureAdminUser();

            try
            {
                return await _cuisineRepository.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting Cuisine.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Cuisine model)
        {
            EnsureAdminUser();

            using (var session = await _mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var success = await _cuisineRepository.UpdateAsync(model, session);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.UpdateCuisineInRecipes(model.Id, model.Name, model.PictureUrl, session);
                    
                    await session.CommitTransactionAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] UpdateAsync :: An error occured while updating Cuisine with id {model.Id}.\n{ex}");

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
                    bool success = await _cuisineRepository.DeleteAsync(id);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.DeleteCuisineInRecipes(id, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] DeleteAsync :: An error occured while deleting Cuisine with id {id}.\n{ex}");

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
