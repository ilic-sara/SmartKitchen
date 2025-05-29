using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly ImageUploadService _imageUploadService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(ImageUploadService imageUploadService, ILogger<FileUploadController> logger)
        {
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file provided");
            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _imageUploadService.UploadImageAsync(stream, file.FileName);
                _logger.LogInformation("Image uploaded: {Url}", url);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Post :: An error occurred while uploading new file.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
