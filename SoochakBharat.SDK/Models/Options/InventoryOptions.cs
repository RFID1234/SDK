using System.Collections.Generic;

namespace SoochakBharat.SDK.Models.Options
{
    public class InventoryOptions
    {
        public IReadOnlyList<int>? Antennas { get; set; }
        public int? Session { get; set; }
        public int? TagPopulation { get; set; }
        public int? ReadDurationMs { get; set; }
        public int? IntervalMs { get; set; }
        public bool IncludeRssi { get; set; } = true;
        public bool IncludePhase { get; set; } = true;
        public bool IncludeFrequency { get; set; } = true;
        public bool IncludeTimestamp { get; set; } = true;
        public bool IncludeBankData { get; set; } = true;
    }
}
