using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Security.Claims;

namespace Services
{
    public interface IMethodService
    {
        Task<List<Method>> GetAllAsync();
        Task<List<Method>> GetMultipleByIdsAsync(List<string> ids);
        Task<Method?> GetOneByIdAsync(string id);
        Task<bool> UpdateAsync(Method model);
        Task<string> InsertAsync(Method model);
        Task<bool> DeleteAsync(string id);
    }


    public class MethodService(IMethodRepository methodRepository,
                               IRecipeRepository recipeRepository,
                               IMongoClient mongoClient,
                               ILogger<MethodService> logger,
                               ClaimsPrincipal user) : IMethodService
    {
        private readonly IMethodRepository _methodRepository = methodRepository ?? throw new ArgumentNullException(nameof(methodRepository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly IMongoClient _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        private readonly ILogger<MethodService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ClaimsPrincipal _user = user ?? throw new ArgumentNullException(nameof(user));

        public async Task<List<Method>> GetAllAsync()
        {
            try
            {
                var methods = await _methodRepository.GetAllAsync();
                return methods.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Methods.\n{ex}");
                throw;
            }
        }

        public async Task<List<Method>> GetMultipleByIdsAsync(List<string> ids)
        {
            try
            {
                return await _methodRepository.GetMultipleByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching Methods with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<Method?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _methodRepository.GetOneByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching Method with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(Method model)
        {
            EnsureAdminUser();

            try
            {
                return await _methodRepository.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting Method.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Method model)
        {
            EnsureAdminUser();

            using (var session = await _mongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var success = await _methodRepository.UpdateAsync(model, session);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.UpdateMethodInRecipes(model.Id, model.Name, model.PictureUrl, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] UpdateAsync :: An error occured while updating Method with id {model.Id}.\n{ex}");

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
                    bool success = await _methodRepository.DeleteAsync(id);

                    if (!success)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }

                    await _recipeRepository.DeleteMethodInRecipes(id, session);

                    await session.CommitTransactionAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                        $"[ERROR] DeleteAsync :: An error occured while deleting Method with id {id}.\n{ex}");

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
