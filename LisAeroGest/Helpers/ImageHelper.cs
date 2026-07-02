using LisAeroGest.Data.Interfaces;

namespace LisAeroGest.Helpers
{
    public class ImageHelper : IImageHelper
    {
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly bool _useAzure;

        public ImageHelper(
            IBlobHelper blobHelper,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _blobHelper = blobHelper;
            _configuration = configuration;
            _env = env;

            // Verifica se o Azure está configurado no appsettings.json
            var connectionString = _configuration["AzureStorage:ConnectionString"];
            _useAzure = !string.IsNullOrWhiteSpace(connectionString);
        }

        public async Task<Guid> UploadImageAsync(IFormFile imageFile, string folder)
        {
            if (imageFile == null || imageFile.Length == 0) return Guid.Empty;

            var imageId = Guid.NewGuid();

            if (_useAzure)
            {
                // Se o Azure estiver configurado, usa o seu BlobHelper
                return await _blobHelper.UploadBlobAsync(imageFile, folder);
            }
            else
            {
                // Se não, guarda localmente em wwwroot/images/{folder}/
                var path = Path.Combine(_env.WebRootPath, "images", folder);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Mantemos a extensão original ou forçamos .jpg
                var extension = Path.GetExtension(imageFile.FileName);
                var fileName = $"{imageId}{extension}";
                var fullPath = Path.Combine(path, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return imageId;
            }
        }

        public async Task DeleteImageAsync(Guid imageId, string folder)
        {
            if (imageId == Guid.Empty) return;

            if (_useAzure)
            {
                await _blobHelper.DeleteBlobAsync(imageId, folder);
            }
            else
            {
                // Elimina o ficheiro local (procura por qualquer extensão)
                var path = Path.Combine(_env.WebRootPath, "images", folder);
                var files = Directory.GetFiles(path, $"{imageId}.*");

                foreach (var file in files)
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }
    }
}
