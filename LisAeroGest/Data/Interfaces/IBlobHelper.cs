namespace LisAeroGest.Data.Interfaces
{
    /// <summary>
    /// Interface para upload e remoção de ficheiros no Azure Blob Storage.
    /// </summary>

    public interface IBlobHelper
    {
        /// <summary>
        /// Faz upload de um ficheiro para o container especificado no Azure Blob Storage.
        /// </summary>
        /// <param name="file">Ficheiro recebido via formulário HTTP.</param>
        /// <param name="containerName">Nome do container de destino no Azure.</param>
        /// <returns>
        /// <see cref="Guid"/> único que identifica o ficheiro no storage
        /// e é guardado na entidade para construir o URL da imagem.
        /// </returns>
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);

        /// <summary>
        /// Faz upload de um array de bytes para o container especificado.
        /// </summary>
        Task<Guid> UploadBlobAsync(byte[] file, string containerName);

        /// <summary>
        /// Faz upload de um stream para o container especificado.
        /// </summary>
        Task<Guid> UploadBlobAsync(Stream stream, string containerName);

        /// <summary>
        /// Remove um ficheiro do container especificado pelo seu GUID.
        /// </summary>
        Task DeleteBlobAsync(Guid id, string containerName);
    }
}
