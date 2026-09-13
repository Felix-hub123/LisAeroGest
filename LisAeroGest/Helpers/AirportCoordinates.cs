namespace LisAeroGest.Helpers
{
    public class AirportCoordinates
    {
        public static readonly Dictionary<string, (double Lat, double Lng)> ByIata = new()
        {
            ["LIS"] = (38.7742, -9.1342),
            ["OPO"] = (41.2481, -8.6814),
            ["FNC"] = (32.6979, -16.7745),
            ["MAD"] = (40.4983, -3.5676),
            ["BCN"] = (41.2974, 2.0833),
            ["CDG"] = (49.0097, 2.5479),
            ["ORY"] = (48.7233, 2.3794),
            ["LHR"] = (51.4700, -0.4543),
            ["AMS"] = (52.3105, 4.7683),
            ["FCO"] = (41.8003, 12.2389),
            ["MXP"] = (45.6306, 8.7281),
            ["GIG"] = (-22.8099, -43.2505),
            ["GRU"] = (-23.4356, -46.4731),
            ["BRU"] = (50.9014, 4.4844),
            ["FRA"] = (50.0379, 8.5622),
            ["MUC"] = (48.3538, 11.7861),
            ["ZRH"] = (47.4582, 8.5555),
            ["GVA"] = (46.2381, 6.1089),
        };

        public static bool TryGet(string? iata, out double lat, out double lng)
        {
            lat = lng = 0;
            if (string.IsNullOrWhiteSpace(iata))
                return false;
            if (!ByIata.TryGetValue(iata.Trim().ToUpperInvariant(), out var p))
                return false;
            lat = p.Lat;
            lng = p.Lng;
            return true;
        }
    }
}
