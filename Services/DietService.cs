using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Security.Claims;

namespace Services
{
    public interface IDietService
    {
        Task<List<Diet>> GetAllAsync();
        Task<List<Diet>> GetMultipleByIdsAsync(List<string> ids);
        Task<Diet?> GetOneByIdAsync(string id);
        Task<bool> UpdateAsync(Diet model);
        Task<string> InsertAsync(Diet model);
        Task<bool> DeleteAsync(string id);
    }


    public class DietService(IDietRepository dietRepository,
                             IRecipeRepository recipeRepository,
                             IMongoClient mongoClient,
                             ILogger<DietService> logger,
                             ClaimsPrincipal user) : IDietService
    {
        private readonly IDietRepository _dietRepository = dietRepository ?? throw new ArgumentNullException(nameof(dietRepository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly IMongoClient _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        private readonly ILogger<DietService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ClaimsPrincipal _user = user ?? throw new ArgumentNullException(nameof(user));

        public async Task<List<Diet>> GetAllAsync()
        {
            try
            {
                var diets = await _dietRepository.GetAllAsync();
                return diets.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Diets.\n{ex}");
                throw;
            }
        }

        public async Task<List<Diet>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                return await _dietRepository.GetMultipleByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching Diets with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<Diet?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _dietRepository.GetOneByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching Diet with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(Diet model)
        {
            EnsureAdminUser();

            try
            {
                return await _dietRepository.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting Diet.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Diet model)
        {
            EnsureAdminUser();

            using (var session = await _mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var success = await _dietRepository.UpdateAsync(model, session);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.UpdateDietInRecipes(model.Id, model.Name, model.PictureUrl, session);

                    await session.CommitTransactionAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] UpdateAsync :: An error occured while updating Diet with id {model.Id}.\n{ex}");

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
                    bool success = await _dietRepository.DeleteAsync(id);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.DeleteDietInRecipes(id, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] DeleteAsync :: An error occured while deleting Diet with id {id}.\n{ex}");

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
