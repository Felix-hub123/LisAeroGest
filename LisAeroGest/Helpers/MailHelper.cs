using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LisAeroGest.Helpers
{
    public class MailHelper : IMailHelper   
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa o MailHelper com a configuração da aplicação.
        /// </summary>
        /// <param name="configuration">Configuração injetada com as definições SMTP.</param>
        /// <returns>
        /// Instância de <see cref="MailHelper"/> pronta a enviar emails
        /// usando as configurações definidas em <c>appsettings.json</c>.
        /// </returns>
        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public Response SendEmail(string to, string subject, string body)
        {
            var nameFrom = _configuration["Mail:NameFrom"] ?? "LisAeroGest";
            var from = _configuration["Mail:From"] ?? string.Empty;
            var smtp = _configuration["Mail:Smtp"] ?? string.Empty;
            var port = int.Parse(_configuration["Mail:Port"] ?? "587");
            var password = _configuration["Mail:Password"] ?? string.Empty;

            // Constrói a mensagem
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            try
            {
                using var client = new SmtpClient();
                client.Connect(smtp, port, SecureSocketOptions.StartTls);
                client.Authenticate(from, password);
                client.Send(message);
                client.Disconnect(quit: true);

                return new Response { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
