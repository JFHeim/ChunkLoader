namespace ChunkLoader.Helpers;

public static class TimeSpanToHumanStr
{
    extension(TimeSpan span)
    {
        /// <summary>
        /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.  Examples: 3.1 seconds, 2 minutes, 4.23 hours, etc.
        /// </summary>
        /// <param name="significantDigits">Significant digits to use for output.</param>
        /// <returns></returns>
        public string ToHumanTimeString(int significantDigits = 3)
        {
            var format = "G" + significantDigits;
            if (span.TotalMilliseconds < 1000) return span.TotalMilliseconds.ToString(format) + " $chunkloader_milliseconds";
            if (span.TotalSeconds      < 60)   return span.TotalSeconds.ToString(format)      + " $chunkloader_seconds";
            if (span.TotalMinutes      < 60)   return span.TotalMinutes.ToString(format)      + " $chunkloader_minutes";
            if (span.TotalHours        < 24)   return span.TotalHours.ToString(format)        + " $chunkloader_hours";

            return span.TotalDays.ToString(format) + " $chunkloader_days";
        }
    }
}