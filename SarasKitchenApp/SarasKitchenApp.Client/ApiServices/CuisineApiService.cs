using Microsoft.JSInterop;
using Models;

public class CuisineApiService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<CuisineApiService> _logger;

    public CuisineApiService(IJSRuntime jsRuntime, ILogger<CuisineApiService> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Cuisine>> GetCuisinesAsync()
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<List<Cuisine>>(
                "apiClient.get",
                "cuisine"
            );

            return responseData is null ? throw new InvalidOperationException("Failed to get all cuisines.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetCuisinesAsync :: An error occured while fetching all cuisines.\n{ex}");
            throw;
        }
    }

    public async Task<Cuisine> GetCuisineByIdAsync(string id)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Cuisine>(
                "apiClient.get",
                $"cuisine/{id}"
            );

            return responseData is null ? throw new InvalidOperationException("Failed to get cuisine with id " + id) : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetCuisineByIdAsync :: An error occured while fetching cuisine with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task<Cuisine> CreateCuisineAsync(Cuisine cuisine)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Cuisine>(
                "apiClient.post",
                "cuisine",
                cuisine
            );

            return responseData is null ? throw new InvalidOperationException("Failed to save cuisine.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] CreateCuisineAsync :: An error occured while creating new cuisine with name {cuisine.Name}.\n{ex}");
            throw;
        }
    }

    public async Task UpdateCuisineAsync(string id, Cuisine cuisine)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.put",
                $"cuisine/{id}",
                cuisine
            );
            _logger.LogInformation($"Cuisine with ID {id} updated successfully.");

        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] UpdateCuisineAsync :: An error occured while updating cuisine with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task DeleteCuisineAsync(string id)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.delete",
                $"cuisine/{id}"
            );
            _logger.LogInformation($"Cuisine with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] DeleteCuisineAsync :: An error occured while deleting cuisine with id {id}.\n{ex}");
            throw;
        }
    }
}
