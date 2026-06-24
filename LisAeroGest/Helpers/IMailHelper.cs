using LisAeroGest.Models;

namespace LisAeroGest.Helpers
{
    public interface IMailHelper
    {
        /// <summary>
        /// Envia um email HTML para o destinatário especificado.
        /// </summary>
        /// <param name="to">Endereço de email do destinatário.</param>
        /// <param name="subject">Assunto do email.</param>
        /// <param name="body">Corpo do email em formato HTML.</param>
        /// <returns>
        /// <see cref="Response"/> indicando se o envio foi bem-sucedido
        /// ou a mensagem de erro caso tenha falhado.
        /// </returns>
        Response SendEmail(string to, string subject, string body);
    }
}
