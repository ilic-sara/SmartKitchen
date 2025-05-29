using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public class ImageUploadService
    {
        private readonly BlobContainerClient _containerClient;

        public ImageUploadService(IConfiguration config)
        {
            var settings = config.GetSection("BlobSettings");
            var connectionString = settings["ConnectionString"];
            var containerName = settings["ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = headers
            });

            return blobClient.Uri.ToString();
        }

    }
}
