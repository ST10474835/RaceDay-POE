using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _client;

        public BlobStorageService(BlobServiceClient client)
        {
            _client = client;
        }

        public async Task<string> UploadAsync(string containerName, IFormFile file, CancellationToken cancellationToken = default)
        {
            var container = _client.GetBlobContainerClient(containerName);

            // A random name stops two uploads with the same file name overwriting each other
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blob = container.GetBlobClient($"{Guid.NewGuid()}{extension}");

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: cancellationToken);

            return blob.Uri.ToString();
        }
    }
}
