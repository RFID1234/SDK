using System;

namespace SoochakBharat.Demo.Desktop.Models
{
    public class TagDisplay
    {
        public string Epc { get; set; } = string.Empty;
        public int Antenna { get; set; }
        public int Rssi { get; set; }
        public int ReadCount { get; set; }
        public int? FrequencyKHz { get; set; }
        public DateTime Timestamp { get; set; }
        
        public string TimestampDisplay => Timestamp.ToString("HH:mm:ss.fff");
    }
}

