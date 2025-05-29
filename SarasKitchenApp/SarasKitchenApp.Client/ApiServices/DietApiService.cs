using Microsoft.JSInterop;
using Models;

public class DietApiService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<DietApiService> _logger;

    public DietApiService(IJSRuntime jsRuntime, ILogger<DietApiService> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Diet>> GetDietsAsync()
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<List<Diet>>(
                "apiClient.get",
                "diet"
            );

            return responseData is null ? throw new InvalidOperationException("Failed to get all diets.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetDietsAsync :: An error occured while fetching all diets.\n{ex}");
            throw;
        }
    }

    public async Task<Diet> GetDietByIdAsync(string id)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Diet>(
                "apiClient.get",
                $"diet/{id}"
            );

            return responseData is null ? throw new InvalidOperationException("Failed to get diet with id " + id) : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetDietByIdAsync :: An error occured while fetching diet with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task<Diet> CreateDietAsync(Diet diet)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Diet>(
                "apiClient.post",
                "diet",
                diet
            );

            return responseData is null ? throw new InvalidOperationException("Failed to save diet.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] CreateDietAsync :: An error occured while creating new diet with name {diet.Name}.\n{ex}");
            throw;
        }
    }

    public async Task UpdateDietAsync(string id, Diet diet)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.put",
                $"diet/{id}",
                diet
            );
            _logger.LogInformation($"Diet with ID {id} updated successfully.");

        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] UpdateDietAsync :: An error occured while updating diet with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task DeleteDietAsync(string id)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.delete",
                $"diet/{id}"
            );
            _logger.LogInformation($"Diet with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] DeleteDietAsync :: An error occured while deleting diet with id {id}.\n{ex}");
            throw;
        }
    }
}
