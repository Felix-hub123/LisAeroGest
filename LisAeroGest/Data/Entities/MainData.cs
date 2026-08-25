using System.Text.Json.Serialization;

namespace LisAeroGest.Data.Entities
{
    public class MainData
    {
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        public int Humidity { get; set; }
    }
}