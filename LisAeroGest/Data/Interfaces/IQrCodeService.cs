namespace LisAeroGest.Data.Interfaces
{
    public interface IQrCodeService
    {
        byte[] GenerateQrCode(string payload);
    }
}
