using System;

namespace Lexim.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset? ToStartOfDay(this DateTimeOffset? value, int timeZone)
        {
            if (value == null)
                return null;

            var result = value.Value.ToOffset(TimeSpan.FromHours(timeZone));
            return new DateTimeOffset(result.Year, result.Month, result.Day, 0, 0, 0, result.Offset);
        }

        public static DateTimeOffset? ToEndOfDay(this DateTimeOffset? value, int timeZone)
        {
            if (value == null)
                return null;

            var result = value.Value.ToOffset(TimeSpan.FromHours(timeZone));
            result = new DateTimeOffset(result.DateTime.Date, result.Offset);
            return result.AddDays(1).AddSeconds(-1);
        }

        public static DateTimeOffset ToStartOfDay(this DateTimeOffset value, int timeZone) =>
            ToStartOfDay((DateTimeOffset?)value, timeZone).Value;

        public static DateTimeOffset ToEndOfDay(this DateTimeOffset value, int timeZone) =>
            ToEndOfDay((DateTimeOffset?)value, timeZone).Value;

        public static DateTime SpecifyKind(this DateTime dt, DateTimeKind kind) => DateTime.SpecifyKind(dt, kind);
    }
}