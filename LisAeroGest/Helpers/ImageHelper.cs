using LisAeroGest.Data.Interfaces;

namespace LisAeroGest.Helpers
{
    /// <summary>

    /// Helper para gestão de imagens — guarda localmente em wwwroot/images/{folder}.

    /// </summary>

    public class ImageHelper : IImageHelper
    {

        private readonly IWebHostEnvironment _env;


        public ImageHelper(IWebHostEnvironment env)
        {

            _env = env;

        }


        public async Task<Guid> UploadImageAsync(IFormFile imageFile, string folder)
        {

            if (imageFile == null || imageFile.Length == 0)

                return Guid.Empty;


            var imageId = Guid.NewGuid();

            var path = Path.Combine(_env.WebRootPath, "images", folder);


            if (!Directory.Exists(path))

                Directory.CreateDirectory(path);


            var extension = Path.GetExtension(imageFile.FileName);

            var fileName = $"{imageId}{extension}";

            var fullPath = Path.Combine(path, fileName);


            using (var stream = new FileStream(fullPath, FileMode.Create))
            {

                await imageFile.CopyToAsync(stream);

            }


            return imageId;

        }


        public async Task DeleteImageAsync(Guid imageId, string folder)
        {

            if (imageId == Guid.Empty) return;


            var path = Path.Combine(_env.WebRootPath, "images", folder);


            if (!Directory.Exists(path)) return;


            var files = Directory.GetFiles(path, $"{imageId}.*");


            foreach (var file in files)
            {

                if (File.Exists(file))

                    File.Delete(file);

            }


            await Task.CompletedTask;

        }


        /// <summary>

        /// Devolve o URL da imagem. Procura o ficheiro real pela extensão.

        /// </summary>

        public string GetImageUrl(Guid imageId, string folder, string placeholderName = "noimage")
        {
            // 1. Se o ID for vazio, vai buscar o placeholder à raiz /images/
            if (imageId == Guid.Empty)
                return $"/images/{placeholderName}.png";

            var path = Path.Combine(_env.WebRootPath, "images", folder);

            if (!Directory.Exists(path))
                return $"/images/{placeholderName}.png";

            var files = Directory.GetFiles(path, $"{imageId}.*");

            if (files.Length > 0)
            {
                var fileName = Path.GetFileName(files[0]);
                return $"/images/{folder}/{fileName}";
            }

            
            return $"/images/{placeholderName}.png";
        }
    }
}
