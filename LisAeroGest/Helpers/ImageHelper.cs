using LisAeroGest.Data.Interfaces;
using Supabase.Storage;

namespace LisAeroGest.Helpers
{
    /// <summary>
    /// Helper para gestão de imagens — Guarda localmente em Desenvolvimento (Visual Studio)
    /// e no Supabase Storage em Produção (Render).
    /// </summary>
    public class ImageHelper : IImageHelper
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _supabaseUrl = "https://glurxiqmtolwtqvspqdt.supabase.co";
        private readonly string _supabaseKey = "sb_publishable_KPpNXNQ59NyIhsxBo_av5Q_qoxL3fsF";
        private readonly string _bucketName = "lisaerogest";

        public ImageHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Faz o upload da imagem e devolve o Guid que SERÁ GUARDADO NO SQL.
        /// </summary>
        public async Task<Guid> UploadImageAsync(IFormFile imageFile, string folder)
        {
            if (imageFile == null || imageFile.Length == 0)
                return Guid.Empty;

            var imageId = Guid.NewGuid();
            var extension = Path.GetExtension(imageFile.FileName);

            // 1. MODO DESENVOLVIMENTO (Visual Studio -> Local wwwroot)
            if (_env.IsDevelopment())
            {
                var path = Path.Combine(_env.WebRootPath, "images", folder);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var fileName = $"{imageId}{extension}";
                var fullPath = Path.Combine(path, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return imageId; // Retorna o Guid para o Controller salvar na BD SQL
            }

            // 2. MODO PRODUÇÃO (Render -> Supabase Storage)
            var filePath = $"{folder}/{imageId}{extension}";

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var storageClient = new Client($"{_supabaseUrl}/storage/v1", new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_supabaseKey}" },
                { "apikey", _supabaseKey }
            });

            var bucket = storageClient.From(_bucketName);
            await bucket.Upload(fileBytes, filePath);

            return imageId; // Retorna o mesmo Guid para o Controller salvar na BD SQL
        }

        /// <summary>
        /// Remove a imagem do local correto conforme o ambiente.
        /// </summary>
        public async Task DeleteImageAsync(Guid imageId, string folder)
        {
            if (imageId == Guid.Empty) return;

            // 1. MODO DESENVOLVIMENTO (Local)
            if (_env.IsDevelopment())
            {
                var path = Path.Combine(_env.WebRootPath, "images", folder);
                if (!Directory.Exists(path)) return;

                var files = Directory.GetFiles(path, $"{imageId}.*");
                foreach (var file in files)
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                return;
            }

            // 2. MODO PRODUÇÃO (Supabase)
            var storageClient = new Client($"{_supabaseUrl}/storage/v1", new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_supabaseKey}" },
                { "apikey", _supabaseKey }
            });

             var bucket = storageClient.From(_bucketName);

                 
            var filesToDelete = new List<string>
            {
                $"{folder}/{imageId}.png",
                $"{folder}/{imageId}.jpg",
                $"{folder}/{imageId}.jpeg"
            };

            await bucket.Remove(filesToDelete);
        }

        /// <summary>
        /// Devolve o URL correto da imagem lendo o Guid da base de dados SQL.
        /// </summary>
        public string GetImageUrl(Guid imageId, string folder, string placeholderName = "noimage")
        {
            if (imageId == Guid.Empty)
                return $"/images/{placeholderName}.png";

            // 1. MODO DESENVOLVIMENTO (Procura no disco local)
            if (_env.IsDevelopment())
            {
                var path = Path.Combine(_env.WebRootPath, "images", folder);
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, $"{imageId}.*");
                    if (files.Length > 0)
                    {
                        var fileName = Path.GetFileName(files[0]);
                        return $"/images/{folder}/{fileName}";
                    }
                }
                return $"/images/{placeholderName}.png";
            }

            // 2. MODO PRODUÇÃO (URL público do Supabase)
            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{folder}/{imageId}.png";
        }
    }
}
