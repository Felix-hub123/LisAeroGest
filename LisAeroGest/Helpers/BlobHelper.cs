using Azure.Storage.Blobs;
using LisAeroGest.Data.Interfaces;

namespace LisAeroGest.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Inicializa o cliente do Azure Blob Storage com a connection string do appsettings.json.
        /// </summary>
        /// <param name="configuration">Configuração da aplicação injetada pelo DI.</param>
        /// <returns>
        /// Instância de <see cref="BlobHelper"/> com o <see cref="BlobServiceClient"/> configurado.
        /// </returns>
        public BlobHelper(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        /// <inheritdoc/>
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            using var stream = file.OpenReadStream();
            return await UploadBlobAsync(stream, containerName);
        }

        /// <inheritdoc/>
        public async Task<Guid> UploadBlobAsync(byte[] file, string containerName)
        {
            using var stream = new MemoryStream(file);
            return await UploadBlobAsync(stream, containerName);
        }

        /// <inheritdoc/>
        public async Task<Guid> UploadBlobAsync(Stream stream, string containerName)
        {
            // Gera um GUID único para identificar o ficheiro no storage
            var guid = Guid.NewGuid();

            // Obtém ou cria o container no Azure
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Faz upload com o GUID como nome do blob
            var blobClient = containerClient.GetBlobClient(guid.ToString());
            await blobClient.UploadAsync(stream, overwrite: true);

            return guid;
        }

        /// <inheritdoc/>
        public async Task DeleteBlobAsync(Guid id, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(id.ToString());
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
