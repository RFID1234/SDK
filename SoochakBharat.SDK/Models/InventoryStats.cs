using System;

namespace SoochakBharat.SDK.Models
{
    /// <summary>
    /// Rolling statistics for inventory performance.
    /// Computed client-side from tag events.
    /// </summary>
    public class InventoryStats
    {
        public string ReaderId { get; set; } = string.Empty;
        public double TagsPerSecond { get; set; }
        public double AverageRssi { get; set; }
        public int ActiveAntennaCount { get; set; }
        public TimeSpan Uptime { get; set; }
        public int UniqueTagCount { get; set; }
        public int TotalReadEvents { get; set; }
    }
}


