using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EaseMyBooking.Api.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloud;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config["CloudSettings:CloudName"] ?? throw new Exception("Missing CloudSettings:CloudName");
        var apiKey = config["CloudSettings:ApiKey"] ?? throw new Exception("Missing CloudSettings:ApiKey");
        var apiSecret = config["CloudSettings:ApiSecret"] ?? throw new Exception("Missing CloudSettings:ApiSecret");

        _cloud = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        _cloud.Api.Secure = true;
    }

    public async Task<(string url, string publicId)> UploadAsync(Stream stream, string fileName, string folder)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,                 // e.g. ease-my-booking/places/123
            Overwrite = false,
            UseFilename = true,
            UniqueFilename = true
        };
        var res = await _cloud.UploadAsync(uploadParams);
        if (res.StatusCode is not System.Net.HttpStatusCode.OK && res.StatusCode is not System.Net.HttpStatusCode.Created)
            throw new Exception($"Cloudinary upload failed: {res.Error?.Message}");
        return (res.SecureUrl?.ToString() ?? "", res.PublicId);
    }

    public async Task DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return;
        var delParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        await _cloud.DestroyAsync(delParams);
    }
}
