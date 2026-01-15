namespace TatehamaATS_v1.Utils;

public static class DateTimeUtils
{

    public static DateTime GetNowJst() => TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
}