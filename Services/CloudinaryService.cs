using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EduAPI.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> SubirVideoAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "eduplatform/videos",
                UniqueFilename = true,
                EagerAsync = true
            });
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }

        public async Task<string> SubirImagenAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "eduplatform/imagenes",
                UniqueFilename = true,
                Transformation = new Transformation().Width(800).Height(450).Crop("fill")
            });
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }

        public async Task<string> SubirFotoPerfilAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "eduplatform/perfiles",
                UniqueFilename = true,
                Transformation = new Transformation().Width(200).Height(200).Crop("fill").Gravity("face")
            });
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }
    }
}
