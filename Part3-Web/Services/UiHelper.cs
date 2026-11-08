namespace RaceDay.Web.Services
{
    public static class UiHelper
    {
        public static string TypeIcon(string eventType) => eventType switch
        {
            "Run" => "\U0001F3C3",
            "Walk" => "\U0001F6B6",
            "Cycle" => "\U0001F6B4",
            _ => "\U0001F3C1"
        };

        public static string Ordinal(int n)
        {
            var rem100 = n % 100;
            if (rem100 is 11 or 12 or 13) return n + "th";

            return (n % 10) switch
            {
                1 => n + "st",
                2 => n + "nd",
                3 => n + "rd",
                _ => n + "th"
            };
        }
    }
}