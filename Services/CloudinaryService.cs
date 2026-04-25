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
    // Validar tamaño antes de subir (100 MB límite plan Free)
    if (file.Length > 100 * 1024 * 1024)
        throw new InvalidOperationException("El video no puede superar 100 MB. Comprime el video antes de subirlo.");

    try
    {
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "eduplatform/videos"
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        
        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new InvalidOperationException($"Error al subir video: {result.Error?.Message}");
            
        return result.SecureUrl.ToString();
    }
    catch (Exception ex) when (ex.Message.Contains("RequestEntityTooLarge"))
    {
        throw new InvalidOperationException("El video es demasiado grande para el plan actual de Cloudinary.");
    }
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
