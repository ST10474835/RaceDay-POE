namespace RaceDay.Api.Helpers
{
    public static class SessionHelper
    {
        public static int? GetUserId(this ISession session) => session.GetInt32("UserID");

        public static string? GetRole(this ISession session) => session.GetString("Role");
    }
}
