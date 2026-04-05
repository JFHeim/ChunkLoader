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
            return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format)  + " $chunkloader_milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format)             + " $chunkloader_seconds"
                    : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format)           + " $chunkloader_minutes"
                        : (span.TotalHours < 24 ? span.TotalHours.ToString(format)           + " $chunkloader_hours"
                            : span.TotalDays.ToString(format)           + " $chunkloader_days")));
        }
    }
}