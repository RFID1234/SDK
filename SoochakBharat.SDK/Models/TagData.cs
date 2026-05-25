using System;

namespace SoochakBharat.SDK.Models
{
    public class TagData
    {
        public string Epc { get; set; } = string.Empty;
        public int Antenna { get; set; }
        public int Rssi { get; set; }
        public int ReadCount { get; set; }
        public int? FrequencyKHz { get; set; }
        public double? Phase { get; set; }
        public string? BankData { get; set; }
        public DateTime TimestampUtc { get; set; }

        public static TagData FromLlrp(LlrpTagReportItem item) => new TagData
        {
            Epc = item.Epc,
            Antenna = item.Antenna,
            Rssi = item.Rssi,
            ReadCount = item.ReadCount,
            FrequencyKHz = item.FrequencyKHz,
            Phase = item.Phase,
            TimestampUtc = item.TimestampUtc
        };

        public static TagData FromSim3500(dynamic tag) => new TagData
        {
            Epc = tag.EPCString,
            Antenna = tag.Antenna,
            Rssi = tag.Rssi,
            ReadCount = tag.ReadCount,
            BankData = tag.EMDDataString,
            FrequencyKHz = tag.Frequency,
            TimestampUtc = DateTime.UtcNow
        };
    }

    public class LlrpTagReportItem
    {
        public string Epc { get; set; } = string.Empty;
        public int Antenna { get; set; }
        public int Rssi { get; set; }
        public int ReadCount { get; set; }
        public int? FrequencyKHz { get; set; }
        public double? Phase { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
