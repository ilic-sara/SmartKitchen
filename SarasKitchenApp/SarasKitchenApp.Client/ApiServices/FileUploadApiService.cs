using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace SarasKitchenApp.Client.ApiServices
{

    public class FileUploadApiService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<FileUploadApiService> _logger;

        public FileUploadApiService(IJSRuntime jsRuntime, ILogger<FileUploadApiService> logger)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);

                var result = await _jsRuntime.InvokeAsync<string>(
                    "apiClient.uploadImage",
                    base64,
                    file.Name,
                    file.ContentType
                );

                return result ?? throw new InvalidOperationException("Image upload returned null.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} [ERROR] UploadImageAsync :: Failed to upload image.\n{ex}");
                throw;
            }
        }
    }
}
