using System;

public static class TimeUtil
{
    public static readonly string TimeFormat = "yyyy-MM-dd HH:mm:ss";		                            //时间格式化字符串
    public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);     //时间起始时间
    public static readonly long BaseTick = BaseTime.Ticks;                                              //其实时间Tick
    private static long mServerTimestamp = 0;                                                           //服务器开始时间(unix时间戳)
    private static long mStartTime = BaseTime.Ticks;                                                    //客户端开始时间
    public static void Initialize(long timestamp) {
        mServerTimestamp = timestamp;
        mStartTime = DateTime.UtcNow.Ticks;
    }
    public static long GetUnix(DateTime time) {
        if (time.Kind != DateTimeKind.Utc) {
            time = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Utc);
        }
        return Convert.ToInt64((time - BaseTime).TotalMilliseconds);
    }
    /* 获得当前unix时间戳*/
    public static long Unix => mServerTimestamp + Convert.ToInt64(new TimeSpan(DateTime.UtcNow.Ticks - mStartTime).TotalMilliseconds);
    public static long LocalUnix => Convert.ToInt64(new TimeSpan(DateTime.UtcNow.Ticks - BaseTick).TotalMilliseconds);
    /* 获得UTC时间 */
    public static DateTime UtcNow => GetUtcDateTime(Unix);
    /* 获得当前时区时间 */
    public static DateTime Now => GetDateTime(Unix);
    /* 获得日历类 */
    public static DateTime GetDateTime(long timestamp) {
        var startTime = BaseTime;
        startTime = startTime.AddMilliseconds(timestamp);
        return TimeZoneInfo.ConvertTime(startTime, TimeZoneInfo.Utc, TimeZoneInfo.Local);
    }
    public static DateTime GetUtcDateTime(long timestamp) {
        var startTime = BaseTime;
        return startTime.AddMilliseconds(timestamp);
    }
    /* 获得当天0点的时间戳 */
    public static long ZeroTime {
        get {
            var now = Now;
            return GetUnix(new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, DateTimeKind.Local)) + 1;
        }
    }
    public static long UtcZeroTime {
        get {
            var now = UtcNow;
            return GetUnix(new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, DateTimeKind.Utc)) + 1;
        }
    }
    /* 判断是不是同一年 */
    public static bool IsSameYear(long time1, long time2)
    {
        var calendar1 = GetDateTime(time1);
        var calendar2 = GetDateTime(time2);
        return (calendar1.Year == calendar2.Year);
    }
    public static bool IsSameYearUtc(long time1, long time2)
    {
        var calendar1 = GetUtcDateTime(time1);
        var calendar2 = GetUtcDateTime(time2);
        return (calendar1.Year == calendar2.Year);
    }
    /* 判断是不是同一个月 */
    public static bool IsSameMonth(long time1, long time2)
    {
        var calendar1 = GetDateTime(time1);
        var calendar2 = GetDateTime(time2);
        return (calendar1.Year == calendar2.Year && calendar1.Month == calendar2.Month);
    }
    public static bool IsSameMonthUtc(long time1, long time2)
    {
        var calendar1 = GetUtcDateTime(time1);
        var calendar2 = GetUtcDateTime(time2);
        return (calendar1.Year == calendar2.Year && calendar1.Month == calendar2.Month);
    }
    /* 判断两个时间戳是不是同一天 */
    public static bool IsSameDay(long time1, long time2)
    {
        var calendar1 = GetDateTime(time1);
        var calendar2 = GetDateTime(time2);
        return (calendar1.Year == calendar2.Year && calendar1.DayOfYear == calendar2.DayOfYear);
    }
    public static bool IsSameDayUtc(long time1, long time2)
    {
        var calendar1 = GetUtcDateTime(time1);
        var calendar2 = GetUtcDateTime(time2);
        return (calendar1.Year == calendar2.Year && calendar1.DayOfYear == calendar2.DayOfYear);
    }
    /* 判断两个时间戳是不是同一小时  小时以下就跟时区没有关系了 */
    public static bool IsSameHour(long time1, long time2)
    {
        var calendar1 = GetDateTime(time1);
        var calendar2 = GetDateTime(time2);
        return (calendar1.Year == calendar2.Year &&
                calendar1.DayOfYear == calendar2.DayOfYear &&
                calendar1.Hour == calendar2.Hour);
    }
    /* 判断两个时间戳是不是同一分钟 */
    public static bool IsSameMinute(long time1, long time2)
    {
        var calendar1 = GetDateTime(time1);
        var calendar2 = GetDateTime(time2);
        return (calendar1.Year == calendar2.Year &&
                calendar1.DayOfYear == calendar2.DayOfYear &&
                calendar1.Hour == calendar2.Hour &&
                calendar1.Minute == calendar2.Minute);
    }

    /* 获得当前时间字符串 */
    public static string GetNowDateString() { return Now.ToString(TimeFormat); }
    /* 获得当前时间字符串 */
    public static string GetNowDateString(string format) { return Now.ToString(format); }
    /* 获得当前时间字符串 */
    public static string GetUtcNowDateString() { return UtcNow.ToString(TimeFormat); }
    /* 获得当前时间字符串 */
    public static string GetUtcNowDateString(string format) { return UtcNow.ToString(format); }


    /* 获得时间字符串 */
    public static string GetDateString(long timestamp) { return GetDateString(timestamp, TimeFormat); }
    /* 获得时间字符串 */
    public static string GetDateString(long timestamp, string format) { return GetDateTime(timestamp).ToString(format); }
    /* 获得时间字符串 */
    public static string GetUtcDateString(long timestamp) { return GetUtcDateString(timestamp, TimeFormat); }
    /* 获得时间字符串 */
    public static string GetUtcDateString(long timestamp, string format) { return GetUtcDateTime(timestamp).ToString(format); }
}