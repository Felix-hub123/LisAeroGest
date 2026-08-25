namespace LisAeroGest.Helpers
{
    public class BookingPricing
    {

        public const decimal ExtraLuggagePrice = 30.00m;
        public const decimal MealIncludedPrice = 15.00m;

        public static decimal CalculateTotal(decimal flightBasePrice, decimal seatPrice, bool extraLuggage, bool mealIncluded)
        {
            decimal total = flightBasePrice + seatPrice;

            if (extraLuggage)
                total += ExtraLuggagePrice;

            if (mealIncluded)
                total += MealIncludedPrice;

            return total;
        }
    }
}
