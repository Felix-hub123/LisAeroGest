namespace LisAeroGest.Data.Interfaces
{
    public interface IImageHelper
    {
        /// <summary>
        /// Faz o upload de uma imagem (Supabase Storage em Produção ou Local em Dev).
        /// </summary>
        /// <param name="imageFile">O ficheiro enviado pelo formulário.</param>
        /// <param name="folder">A pasta de destino (ex: "aircraft", "airline", "airport").</param>
        /// <returns>O Guid da imagem guardada na base de dados.</returns>
        Task<Guid> UploadImageAsync(IFormFile imageFile, string folder);

        /// <summary>
        /// Elimina uma imagem (Supabase Storage em Produção ou Local em Dev).
        /// </summary>
        /// <param name="imageId">O Guid da imagem.</param>
        /// <param name="folder">A pasta onde a imagem está guardada.</param>
        Task DeleteImageAsync(Guid imageId, string folder);

        /// <summary>
        /// Devolve o URL completo da imagem (URL público Supabase ou caminho local). 
        /// Se o ID for vazio, devolve o placeholder estático.
        /// </summary>
        /// <param name="imageId">O Guid da imagem.</param>
        /// <param name="folder">A pasta onde a imagem está guardada.</param>
        /// <param name="placeholderName">Nome da imagem padrão caso não exista ID (default: "noimage").</param>
        string GetImageUrl(Guid imageId, string folder, string placeholderName = "noimage");
    }
}

