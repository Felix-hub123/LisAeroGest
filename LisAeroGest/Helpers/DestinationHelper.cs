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
                    ImageUrl = "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "CDG",
                    City = "Paris",
                    Country = "França",
                    ImageUrl = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "LHR",
                    City = "Londres",
                    Country = "Reino Unido",
                    ImageUrl = "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "JFK",
                    City = "Nova Iorque",
                    Country = "Estados Unidos",
                    ImageUrl = "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "FCO",
                    City = "Roma",
                    Country = "Itália",
                    ImageUrl = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "BCN",
                    City = "Barcelona",
                    Country = "Espanha",
                    ImageUrl = "https://images.unsplash.com/photo-1583422409516-2895a77efded?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "AMS",
                    City = "Amesterdão",
                    Country = "Países Baixos",
                    ImageUrl = "https://images.unsplash.com/photo-1534351590666-13e3e96b5017?w=600&q=80"
                },

                new DestinationInfo
                {
                    Code = "FRA",
                    City = "Frankfurt",
                    Country = "Alemanha",
                    ImageUrl = "https://images.unsplash.com/photo-1577985043696-8bd54d9f093f?w=600&q=80"
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