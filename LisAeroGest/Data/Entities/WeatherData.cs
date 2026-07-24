using Org.BouncyCastle.Asn1.Pkcs;

namespace LisAeroGest.Data.Entities
{
    public class WeatherData
    {
        public MainData? Main { get; set; }
        public List<WeatherDescription>? Weather { get; set; }
        public WindData? Wind { get; set; }
        public int Visibility { get; set; }
        public string? Name { get; set; }
    }
}
