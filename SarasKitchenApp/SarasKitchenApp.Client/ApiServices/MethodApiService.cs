using Microsoft.JSInterop;
using Models;

public class MethodApiService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<MethodApiService> _logger;

    public MethodApiService(IJSRuntime jsRuntime, ILogger<MethodApiService> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Method>> GetMethodsAsync()
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<List<Method>>(
                "apiClient.get",
                "method"
            );

            return responseData is null ? throw new InvalidOperationException("Failed to get all methods.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetMethodsAsync :: An error occured while fetching all methods.\n{ex}");
            throw;
        }
    }

    public async Task<Method> GetMethodByIdAsync(string id)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Method>(
                "apiClient.get",
                $"method/{id}"
            );

            if (responseData is null)
            {
                throw new InvalidOperationException("Failed to get method with id " + id);
            }
            return responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] GetMethodByIdAsync :: An error occured while fetching method with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task<Method> CreateMethodAsync(Method method)
    {
        try
        {
            var responseData = await _jsRuntime.InvokeAsync<Method>(
                "apiClient.post",
                "method",
                method
            );

            return responseData is null ? throw new InvalidOperationException("Failed to save method.") : responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] CreateMethodAsync :: An error occured while creating new method with name {method.Name}.\n{ex}");
            throw;
        }
    }

    public async Task UpdateMethodAsync(string id, Method method)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.put",
                $"method/{id}",
                method
            );
            _logger.LogInformation($"Method with ID {id} updated successfully.");

        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] UpdateMethodAsync :: An error occured while updating method with id {id}.\n{ex}");
            throw;
        }
    }

    public async Task DeleteMethodAsync(string id)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "apiClient.delete",
                $"method/{id}"
            );
            _logger.LogInformation($"Method with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                   $"[ERROR] DeleteMethodAsync :: An error occured while deleting method with id {id}.\n{ex}");
            throw;
        }
    }
}
