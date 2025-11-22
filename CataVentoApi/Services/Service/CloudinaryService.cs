using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using CataVentoApi.Entity;

namespace CataVentoApi.Services.Service
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Error != null)
                {
                    return false;
                }

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),

                    Folder = "cata-vento-app",

                };

                // Chamada assíncrona para a API de upload, usando as credenciais secretas
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    // Se houver erro, você pode logar ou lançar uma exceção mais detalhada
                    throw new Exception(uploadResult.Error.Message);
                }

                return uploadResult;
            }
        }
    }
}
