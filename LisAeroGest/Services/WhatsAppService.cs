using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using LisAeroGest.Data.Interfaces;

namespace LisAeroGest.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IConfiguration config, ILogger<WhatsAppService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendTicketMessageAsync(string phoneNumber, string message)
        {
            var sid = _config["Twilio:AccountSid"];
            var token = _config["Twilio:AuthToken"];
            var from = _config["Twilio:WhatsAppFrom"];

            if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("Twilio não está configurado.");
                return false;
            }

            var digits = new string((phoneNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length < 9)
                return false;

            if (!digits.StartsWith("351") && digits.Length == 9)
                digits = "351" + digits;

            try
            {
                TwilioClient.Init(sid, token);
                await MessageResource.CreateAsync(
                    from: new PhoneNumber(from),
                    to: new PhoneNumber($"whatsapp:+{digits}"),
                    body: message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar WhatsApp.");
                return false;
            }
        }
    }
}
