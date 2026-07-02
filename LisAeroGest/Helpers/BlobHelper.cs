using Azure.Storage.Blobs;
using LisAeroGest.Data.Interfaces;

namespace LisAeroGest.Helpers
{

    /// <summary>
    /// Implementação do helper para Azure Blob Storage.
    /// Se a connection string não estiver configurada, o upload é ignorado
    /// e a entidade fica com ImageId vazio — usando a imagem noimage.png local.
    /// </summary>
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly bool _isConfigured;

        /// <summary>
        /// Inicializa o BlobHelper verificando se o Azure Blob Storage está configurado.
        /// </summary>
        /// <param name="configuration">Configuração da aplicação injetada pelo DI.</param>
        /// <returns>
        /// Instância de <see cref="BlobHelper"/> configurada para usar o Azure
        /// se a connection string estiver definida, ou modo local caso contrário.
        /// </returns>
        public BlobHelper(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _isConfigured = !string.IsNullOrWhiteSpace(connectionString);

            if (_isConfigured)
                _blobServiceClient = new BlobServiceClient(connectionString);
        }

       
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            if (!_isConfigured) return Guid.Empty;

            using var stream = file.OpenReadStream();
            return await UploadBlobAsync(stream, containerName);
        }

      
        public async Task<Guid> UploadBlobAsync(byte[] file, string containerName)
        {
            if (!_isConfigured) return Guid.Empty;

            using var stream = new MemoryStream(file);
            return await UploadBlobAsync(stream, containerName);
        }

       
        public async Task<Guid> UploadBlobAsync(Stream stream, string containerName)
        {
            if (!_isConfigured) return Guid.Empty;

            var guid = Guid.NewGuid();

            var containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(guid.ToString());
            await blobClient.UploadAsync(stream, overwrite: true);

            return guid;
        }

       
        public async Task DeleteBlobAsync(Guid id, string containerName)
        {
            if (!_isConfigured) return;

            var containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(id.ToString());
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
