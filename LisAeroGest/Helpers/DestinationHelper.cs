namespace LisAeroGest.Helpers
{
    public static class DestinationHelper
    {
        public static List<DestinationInfo> GetFeaturedDestinations()
        {
            return new List<DestinationInfo>
            {
                new DestinationInfo
                {
                    Code = "MAD",
                    City = "Madrid",
                    Country = "Espanha",
                    ImageUrl = "/images/destinations/madrid.jpg"
                },

                new DestinationInfo
                {
                    Code = "CDG",
                    City = "Paris",
                    Country = "França",
                    ImageUrl = "/images/destinations/paris.jpg"
                },

                new DestinationInfo
                {
                    Code = "LHR",
                    City = "Londres",
                    Country = "Reino Unido",
                    ImageUrl = "/images/destinations/london.jpg"
                },

                new DestinationInfo
                {
                    Code = "JFK",
                    City = "Nova Iorque",
                    Country = "Estados Unidos",
                    ImageUrl = "/images/destinations/newyork.jpg"
                }
            };
        }
    }

    public class DestinationInfo
    {
        public string Code { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
}