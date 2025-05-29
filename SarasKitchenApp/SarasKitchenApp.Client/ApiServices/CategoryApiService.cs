using Microsoft.JSInterop;
using Models;

namespace SarasKitchenApp.Client.ApiServices
{
    public class CategoryApiService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<CategoryApiService> _logger;

        public CategoryApiService(IJSRuntime jsRuntime, ILogger<CategoryApiService> logger)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<List<Category>>(
                    "apiClient.get",
                    "category"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get all categories.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetCategoriesAsync :: An error occured while fetching all categories.\n{ex}");
                throw;
            }
        }

        public async Task<Category> GetCategoryByIdAsync(string id)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<Category>(
                    "apiClient.get",
                    $"{"category"}/{id}"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get category with id " + id) : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetCategoryByIdAsync :: An error occured while fetching category with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<Category>(
                    "apiClient.post",
                    "category",
                    category
                );

                return responseData is null ? throw new InvalidOperationException("Failed to save category.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] CreateCategoryAsync :: An error occured while creating new category with name {category.Name}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateCategoryAsync(string id, Category category)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "apiClient.put",
                    $"{"category"}/{id}",
                    category 
                );
                _logger.LogInformation($"Category with ID {id} updated successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] UpdateCategoryAsync :: An error occured while updating category with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task DeleteCategoryAsync(string id)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "apiClient.delete",
                    $"{"category"}/{id}"
                );
                _logger.LogInformation($"Category with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] DeleteCategoryAsync :: An error occured while deleting category with id {id}.\n{ex}");
                throw;
            }
        }
    }
}
