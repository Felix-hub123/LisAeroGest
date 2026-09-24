using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace LisAeroGest.Mobile.Helpers
{
    /// <summary>
    /// Gera QR Codes como arrays de bytes PNG, prontos a serem
    /// mostrados num <c>ImageSource.FromStream(...)</c>.
    /// </summary>
    public static class QrCodeGenerator
    {
        public static byte[] GeneratePng(string content, int size = 400)
        {
            if (string.IsNullOrWhiteSpace(content))
                return Array.Empty<byte>();

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = size,
                    Width = size,
                    Margin = 1,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(content);

            using var bitmap = new SkiaSharp.SKBitmap(
                pixelData.Width,
                pixelData.Height,
                SkiaSharp.SKColorType.Bgra8888,
                SkiaSharp.SKAlphaType.Premul);

            System.Runtime.InteropServices.Marshal.Copy(
                pixelData.Pixels,
                0,
                bitmap.GetPixels(),
                pixelData.Pixels.Length);

            using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
            using var data = image.Encode(
                SkiaSharp.SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }
    }
}