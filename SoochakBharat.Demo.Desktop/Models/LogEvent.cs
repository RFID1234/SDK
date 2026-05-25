using System;

namespace SoochakBharat.Demo.Desktop.Models
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = "UI";
    }
}


