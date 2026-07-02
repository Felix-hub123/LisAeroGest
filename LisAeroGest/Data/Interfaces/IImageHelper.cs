namespace LisAeroGest.Data.Interfaces
{
    public interface IImageHelper
    {
        /// <summary>
        /// Faz o upload de uma imagem (Azure ou Local).
        /// </summary>
        /// <param name="imageFile">O ficheiro enviado pelo formulário.</param>
        /// <param name="folder">A pasta de destino (ex: "airlines", "airports").</param>
        /// <returns>O Guid da imagem guardada.</returns>
        Task<Guid> UploadImageAsync(IFormFile imageFile, string folder);

        /// <summary>
        /// Elimina uma imagem (Azure ou Local).
        /// </summary>
        /// <param name="imageId">O Guid da imagem.</param>
        /// <param name="folder">A pasta onde a imagem está guardada.</param>
        Task DeleteImageAsync(Guid imageId, string folder);
    }
}

