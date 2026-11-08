namespace RaceDay.Api.Services
{
    public interface IBlobStorageService
    {
        /// <summary>Uploads an image to the named container and returns its public URL.</summary>
        Task<string> UploadAsync(string containerName, IFormFile file, CancellationToken cancellationToken = default);
    }
}
