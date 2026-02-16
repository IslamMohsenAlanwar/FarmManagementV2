public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo EgyptTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

    public static DateTime ToEgyptTime(DateTime utcDate)
    {
        if (utcDate.Kind == DateTimeKind.Unspecified)
            utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, EgyptTimeZone);
    }

    public static string GetArabicDayName(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الإثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => ""
        };
    }
}
