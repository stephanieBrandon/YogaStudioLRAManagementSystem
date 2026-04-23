namespace YogaStudioLRAManagementSystem.Helpers
{
    public static class DateHelper
    {
        private static readonly TimeZoneInfo EasternTime =
            TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, EasternTime);

        public static DateTime Today => Now.Date;

        /// <summary>
        /// Converts a UTC datetime (or Unspecified, assumed UTC) to Eastern Time for display.
        /// Use on stored datetimes from the database when displaying to users.
        /// </summary>
        public static DateTime ToEastern(this DateTime dateTime)
        {
            var asUtc = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => dateTime
            };
            return TimeZoneInfo.ConvertTimeFromUtc(asUtc, EasternTime);
        }
    }
}