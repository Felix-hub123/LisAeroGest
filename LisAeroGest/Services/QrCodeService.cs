using LisAeroGest.Data.Interfaces;
using QRCoder;

namespace LisAeroGest.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCode(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return Array.Empty<byte>();

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
